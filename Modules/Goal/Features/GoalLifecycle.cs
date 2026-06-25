using backend.Data;
using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Services;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Features;

public record GetGoalPeriodsQuery(Guid UserId, Guid GoalId, GoalPagination Pagination) : IRequest<GoalPageResponse<GoalPeriodResponse>>;
public record GetGoalProgressQuery(Guid UserId, Guid GoalId, GoalPagination Pagination) : IRequest<GoalPageResponse<GoalProgressEntryResponse>>;
public record UpdateGoalCommand(Guid UserId, Guid GoalId, double TargetValue, bool IsPublic) : IRequest;
public record ArchiveGoalCommand(Guid UserId, Guid GoalId) : IRequest;

public class GetGoalPeriodsHandler : IRequestHandler<GetGoalPeriodsQuery, GoalPageResponse<GoalPeriodResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;

    public GetGoalPeriodsHandler(FitspireDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GoalPageResponse<GoalPeriodResponse>> Handle(GetGoalPeriodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.GoalPeriods.Where(period => period.GoalId == request.GoalId && period.Goal.UserId == request.UserId);
        var totalCount = await query.CountAsync(cancellationToken);
        var periods = await query.OrderByDescending(period => period.StartAt).Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize).ToListAsync(cancellationToken);
        return new GoalPageResponse<GoalPeriodResponse>(_mapper.Map<List<GoalPeriodResponse>>(periods), request.Pagination.Page,
            request.Pagination.PageSize, totalCount);
    }
}

public class GetGoalProgressHandler : IRequestHandler<GetGoalProgressQuery, GoalPageResponse<GoalProgressEntryResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;

    public GetGoalProgressHandler(FitspireDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GoalPageResponse<GoalProgressEntryResponse>> Handle(GetGoalProgressQuery request, CancellationToken cancellationToken)
    {
        var query = _context.GoalProgressEntries.Where(entry => entry.GoalId == request.GoalId && entry.Goal.UserId == request.UserId);
        var totalCount = await query.CountAsync(cancellationToken);
        var entries = await query.OrderByDescending(entry => entry.RecordedAt).Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize).ToListAsync(cancellationToken);
        return new GoalPageResponse<GoalProgressEntryResponse>(_mapper.Map<List<GoalProgressEntryResponse>>(entries), request.Pagination.Page,
            request.Pagination.PageSize, totalCount);
    }
}

public class UpdateGoalHandler : IRequestHandler<UpdateGoalCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notifications;
    private readonly IGoalTransactionService _transactions;

    public UpdateGoalHandler(FitspireDbContext context, IUnitOfWork unitOfWork, INotificationService notifications,
        IGoalTransactionService transactions)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _transactions = transactions;
    }

    public async Task Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        await _transactions.ExecuteAsync(async token =>
        {
            await UpdateAsync(request, token);
            return true;
        }, cancellationToken);
    }

    private async Task UpdateAsync(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals.Include(item => item.Periods).Include(item => item.GoalType)
            .FirstOrDefaultAsync(item => item.Id == request.GoalId && item.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("Goal not found.");
        goal.UpdateTarget(request.TargetValue, request.IsPublic);
        foreach (var period in goal.Periods.Where(item => item.Status == "Active"))
        {
            var wasActive = period.Status == "Active";
            period.UpdateTarget(request.TargetValue);
            goal.ApplyCurrentPeriodProgress(period.ProgressValue, period.Status == "Completed");
            if (wasActive && period.Status == "Completed")
                await _notifications.CreateAsync(goal.UserId, NotificationType.GoalCompleted,
                    $"You completed your goal: {goal.GoalType.Name}.", referenceEntityId: goal.Id,
                    referenceEntityType: NotificationReferenceTypes.Goal, cancellationToken: cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ArchiveGoalHandler : IRequestHandler<ArchiveGoalCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoalTransactionService _transactions;

    public ArchiveGoalHandler(FitspireDbContext context, IUnitOfWork unitOfWork, IGoalTransactionService transactions)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _transactions = transactions;
    }

    public async Task Handle(ArchiveGoalCommand request, CancellationToken cancellationToken)
    {
        await _transactions.ExecuteAsync(async token =>
        {
            await ArchiveAsync(request, token);
            return true;
        }, cancellationToken);
    }

    private async Task ArchiveAsync(ArchiveGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals.FirstOrDefaultAsync(item => item.Id == request.GoalId && item.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("Goal not found.");
        goal.Archive();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
