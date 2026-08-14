using AutoMapper;
using backend.Data;
using backend.Modules.Auth.Authorization;
using backend.Modules.Media.Domain;
using backend.Modules.Moderation.Contracts;
using backend.Modules.Moderation.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.User.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace backend.Modules.Moderation.Services;

public sealed class AdminModerationService : IAdminModerationService
{
    private readonly FitspireDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IModerationMediaPreviewService _mediaPreviewService;
    private readonly IMapper _mapper;

    public AdminModerationService(FitspireDbContext context, UserManager<AppUser> userManager,
        IModerationMediaPreviewService mediaPreviewService, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mediaPreviewService = mediaPreviewService;
        _mapper = mapper;
    }

    public async Task<AdminModerationQueueSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _context.ModerationReports.AsNoTracking()
            .GroupBy(report => report.Status)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        return new AdminModerationQueueSummaryResponse(
            counts.SingleOrDefault(count => count.Key == ModerationReportStatus.Open)?.Count ?? 0,
            counts.SingleOrDefault(count => count.Key == ModerationReportStatus.Resolved)?.Count ?? 0);
    }

    public async Task<AdminModerationReportPageResponse> GetReportsAsync(AdminModerationReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationReports.AsNoTracking()
            .Include(report => report.ReporterUser)
            .Include(report => report.SubjectUser)
            .AsQueryable();
        query = ApplyFilter(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);
        var reports = await query.OrderByDescending(report => report.CreatedAt).ThenByDescending(report => report.Id)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(cancellationToken);
        var removedTargets = await GetRemovedTargetIdsAsync(reports, cancellationToken);
        var items = reports.Select(report => new AdminModerationReportListItemResponse(report.Id, report.Status,
            report.TargetType, report.Reason, report.CreatedAt, MapUser(report.ReporterUser), MapUser(report.SubjectUser),
            removedTargets.Contains((report.TargetType, report.TargetId)))).ToList();
        return new AdminModerationReportPageResponse(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<AdminModerationReportDetailResponse> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var report = await LoadReportAsync(reportId, false, cancellationToken);
        return await MapDetailAsync(report, cancellationToken);
    }

    public async Task<AdminModerationReportDetailResponse> ResolveAsync(Guid moderatorUserId, Guid reportId,
        ResolveModerationReportRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var report = await LoadReportAsync(reportId, true, cancellationToken);
        var utcNow = DateTime.UtcNow;
        await EnsureModeratorCanActOnSubjectAsync(moderatorUserId, report.SubjectUserId, report.SubjectUser, cancellationToken);
        if (report.Status != ModerationReportStatus.Open)
            throw new ConflictException("This moderation report has already been resolved.");

        if (request.Action == AdminModerationResolutionAction.Dismiss)
        {
            report.Resolve(ModerationResolutionOutcome.Dismissed, moderatorUserId, request.ModeratorNote, utcNow);
            AddAction(report, moderatorUserId, ModerationActionType.ReportDismissed, request.ModeratorNote, null, utcNow);
        }
        else
        {
            var removeTarget = request.Action is AdminModerationResolutionAction.RemoveTarget or AdminModerationResolutionAction.RemoveTargetAndSuspendUser;
            var suspendUser = request.Action is AdminModerationResolutionAction.SuspendUser or AdminModerationResolutionAction.RemoveTargetAndSuspendUser;
            if (removeTarget)
                await RemoveTargetAsync(report, moderatorUserId, request, suspendUser, utcNow, cancellationToken);
            if (suspendUser)
                SuspendSubject(report, moderatorUserId, request, utcNow);

            report.Resolve(ModerationResolutionOutcome.ActionTaken, moderatorUserId, request.ModeratorNote, utcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await MapDetailAsync(report, cancellationToken);
    }

    public async Task<AdminModerationReportDetailResponse> RestoreTargetAsync(Guid moderatorUserId, Guid reportId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var report = await LoadReportAsync(reportId, true, cancellationToken);
        await EnsureModeratorCanActOnSubjectAsync(moderatorUserId, report.SubjectUserId, report.SubjectUser, cancellationToken);
        var utcNow = DateTime.UtcNow;
        if (!await RestoreTargetAsync(report, utcNow, cancellationToken))
            throw new ConflictException("This report target is not currently removed by moderation.");

        AddAction(report, moderatorUserId, ModerationActionType.ContentRestored, null, null, utcNow);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await MapDetailAsync(report, cancellationToken);
    }

    public async Task<AdminModerationReportDetailResponse> UnsuspendUserAsync(Guid moderatorUserId, Guid reportId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var report = await LoadReportAsync(reportId, true, cancellationToken);
        if (moderatorUserId == report.SubjectUserId)
            throw new DomainException("Moderators cannot change their own suspension state.");

        var utcNow = DateTime.UtcNow;
        if (report.SubjectUser.IsSuspended(utcNow))
        {
            report.SubjectUser.Unsuspend(utcNow);
            AddAction(report, moderatorUserId, ModerationActionType.UserUnsuspended, null, null, utcNow);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await MapDetailAsync(report, cancellationToken);
    }

    private async Task RemoveTargetAsync(ModerationReport report, Guid moderatorUserId, ResolveModerationReportRequest request,
        bool canContinueWithoutTarget, DateTime utcNow, CancellationToken cancellationToken)
    {
        var removed = await SetTargetRemovalAsync(report, true, utcNow, cancellationToken);
        if (!removed && !canContinueWithoutTarget)
            throw new NotFoundException("The report target no longer exists.");
        if (removed)
            AddAction(report, moderatorUserId, ModerationActionType.ContentRemoved, request.ModeratorNote, null, utcNow);
    }

    private void SuspendSubject(ModerationReport report, Guid moderatorUserId, ResolveModerationReportRequest request, DateTime utcNow)
    {
        var untilUtc = utcNow.AddDays(request.SuspensionDurationDays!.Value);
        report.SubjectUser.Suspend(untilUtc, request.ModeratorNote, utcNow);
        AddAction(report, moderatorUserId, ModerationActionType.UserSuspended, request.ModeratorNote, untilUtc, utcNow);
    }

    private async Task<bool> SetTargetRemovalAsync(ModerationReport report, bool remove, DateTime utcNow,
        CancellationToken cancellationToken)
    {
        switch (report.TargetType)
        {
            case ModerationReportTargetType.Post:
            {
                var post = await _context.Posts.FirstOrDefaultAsync(item => item.Id == report.TargetId, cancellationToken);
                if (post is null) return false;
                if (remove) post.RemoveByModeration(utcNow); else post.RestoreByModeration(utcNow);
                return remove ? post.IsModerationRemoved : !post.IsModerationRemoved;
            }
            case ModerationReportTargetType.Comment:
            {
                var comment = await _context.Comments.FirstOrDefaultAsync(item => item.Id == report.TargetId, cancellationToken);
                if (comment is null) return false;
                if (remove) comment.RemoveByModeration(utcNow); else comment.RestoreByModeration(utcNow);
                return remove ? comment.IsModerationRemoved : !comment.IsModerationRemoved;
            }
            case ModerationReportTargetType.Media:
            {
                var media = await _context.MediaAssets.Include(item => item.Variants)
                    .FirstOrDefaultAsync(item => item.Id == report.TargetId, cancellationToken);
                if (media is null) return false;
                if (remove) media.RemoveByModeration(utcNow); else media.RestoreByModeration(utcNow);
                return remove ? media.IsModerationRemoved : !media.IsModerationRemoved;
            }
            case ModerationReportTargetType.Profile:
                throw new DomainException("Profiles cannot be removed by this moderation workflow.");
            default:
                throw new DomainException("Unsupported moderation report target.");
        }
    }

    private async Task<bool> RestoreTargetAsync(ModerationReport report, DateTime utcNow, CancellationToken cancellationToken)
    {
        var before = await GetTargetStateAsync(report, cancellationToken);
        if (!before.Exists || !before.IsRemoved)
            return false;
        return await SetTargetRemovalAsync(report, false, utcNow, cancellationToken);
    }

    private async Task<AdminModerationReportDetailResponse> MapDetailAsync(ModerationReport report, CancellationToken cancellationToken)
    {
        var target = await GetTargetStateAsync(report, cancellationToken);
        var actionEntities = await _context.ModerationActions.AsNoTracking()
            .Include(action => action.ModeratorUser)
            .Where(action => action.ReportId == report.Id)
            .OrderByDescending(action => action.OccurredAtUtc)
            .ToListAsync(cancellationToken);
        var actions = _mapper.Map<List<AdminModerationActionResponse>>(actionEntities);
        return new AdminModerationReportDetailResponse(report.Id, report.Status, report.ResolutionOutcome, report.TargetType,
            report.TargetId, report.MediaContext, report.Reason, report.Details, report.CreatedAt, MapUser(report.ReporterUser),
            MapUser(report.SubjectUser), report.TargetSnapshotJson, target, report.SubjectUser.SuspendedUntilUtc, actions);
    }

    private async Task<ModerationTargetState> GetTargetStateAsync(ModerationReport report, CancellationToken cancellationToken)
    {
        if (report.TargetType == ModerationReportTargetType.Profile)
            return new ModerationTargetState(true, false, report.SubjectUser.Bio, null, MapUser(report.SubjectUser), null);

        if (report.TargetType == ModerationReportTargetType.Post)
        {
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == report.TargetId, cancellationToken);
            return post is null ? ModerationTargetState.Missing : new ModerationTargetState(true, post.IsModerationRemoved, post.Content, post.Id, null, null);
        }

        if (report.TargetType == ModerationReportTargetType.Comment)
        {
            var comment = await _context.Comments.AsNoTracking().FirstOrDefaultAsync(item => item.Id == report.TargetId, cancellationToken);
            return comment is null ? ModerationTargetState.Missing : new ModerationTargetState(true, comment.IsModerationRemoved, comment.Content, comment.PostId, null, null);
        }

        var media = await _context.MediaAssets.AsNoTracking().Include(item => item.Variants)
            .FirstOrDefaultAsync(item => item.Id == report.TargetId, cancellationToken);
        if (media is null)
            return ModerationTargetState.Missing;
        var preview = await _mediaPreviewService.CreateAsync(media, cancellationToken);
        return new ModerationTargetState(true, media.IsModerationRemoved, null, null, null, preview);
    }

    private async Task<ModerationReport> LoadReportAsync(Guid reportId, bool tracked, CancellationToken cancellationToken)
    {
        IQueryable<ModerationReport> query = _context.ModerationReports
            .Include(report => report.ReporterUser)
            .Include(report => report.SubjectUser)
            .Include(report => report.Actions).ThenInclude(action => action.ModeratorUser);
        if (!tracked)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(report => report.Id == reportId, cancellationToken)
               ?? throw new NotFoundException("Moderation report was not found.");
    }

    private async Task<HashSet<(ModerationReportTargetType, Guid)>> GetRemovedTargetIdsAsync(IReadOnlyCollection<ModerationReport> reports,
        CancellationToken cancellationToken)
    {
        var postIds = reports.Where(report => report.TargetType == ModerationReportTargetType.Post).Select(report => report.TargetId).ToArray();
        var commentIds = reports.Where(report => report.TargetType == ModerationReportTargetType.Comment).Select(report => report.TargetId).ToArray();
        var mediaIds = reports.Where(report => report.TargetType == ModerationReportTargetType.Media).Select(report => report.TargetId).ToArray();
        var removed = new HashSet<(ModerationReportTargetType, Guid)>();
        removed.UnionWith((await _context.Posts.AsNoTracking().Where(post => postIds.Contains(post.Id) && post.ModerationRemovedAtUtc != null)
            .Select(post => post.Id).ToListAsync(cancellationToken)).Select(id => (ModerationReportTargetType.Post, id)));
        removed.UnionWith((await _context.Comments.AsNoTracking().Where(comment => commentIds.Contains(comment.Id) && comment.ModerationRemovedAtUtc != null)
            .Select(comment => comment.Id).ToListAsync(cancellationToken)).Select(id => (ModerationReportTargetType.Comment, id)));
        removed.UnionWith((await _context.MediaAssets.AsNoTracking().Where(media => mediaIds.Contains(media.Id) && media.ModerationRemovedAtUtc != null)
            .Select(media => media.Id).ToListAsync(cancellationToken)).Select(id => (ModerationReportTargetType.Media, id)));
        return removed;
    }

    private static IQueryable<ModerationReport> ApplyFilter(IQueryable<ModerationReport> query, AdminModerationReportFilter filter)
    {
        if (filter.Status is not null) query = query.Where(report => report.Status == filter.Status);
        if (filter.TargetType is not null) query = query.Where(report => report.TargetType == filter.TargetType);
        if (filter.Reason is not null) query = query.Where(report => report.Reason == filter.Reason);
        return query;
    }

    private async Task EnsureModeratorCanActOnSubjectAsync(Guid moderatorUserId, Guid subjectUserId, AppUser subjectUser,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (moderatorUserId == subjectUserId)
            throw new DomainException("Moderators cannot take moderation action against themselves.");
        if (await _userManager.IsInRoleAsync(subjectUser, AppRoles.Admin))
            throw new DomainException("Moderation actions cannot suspend or remove content from an administrator.");
    }

    private void AddAction(ModerationReport report, Guid moderatorUserId, ModerationActionType actionType, string? note,
        DateTime? suspensionEndsAtUtc, DateTime utcNow) => _context.ModerationActions.Add(ModerationAction.Create(report.Id,
        moderatorUserId, report.SubjectUserId, report.TargetType, report.TargetId, actionType, note, suspensionEndsAtUtc, utcNow));

    private static AdminModerationUserResponse MapUser(AppUser user) => new(user.Id, user.UserName ?? string.Empty, user.DisplayName);

    private sealed record ModerationTargetState(bool Exists, bool IsRemoved, string? Content, Guid? PostId,
        AdminModerationUserResponse? Profile, backend.Modules.Media.Contracts.MediaResponse? Media)
    {
        public static ModerationTargetState Missing { get; } = new(false, false, null, null, null, null);
        public static implicit operator AdminModerationTargetResponse(ModerationTargetState value) => new(value.Exists, value.IsRemoved,
            value.Content, value.PostId, value.Profile, value.Media);
    }
}
