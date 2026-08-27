using AutoMapper;
using backend.Data;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachInteractionService
{
    Task<CoachThreadResponse> CreateThreadAsync(Guid userId, CreateCoachThreadRequest request, CancellationToken cancellationToken);
    Task<CoachThreadPageResponse> GetThreadsAsync(Guid userId, CoachThreadHistoryFilter filter, CancellationToken cancellationToken);
    Task<CoachThreadResponse> GetThreadAsync(Guid userId, Guid threadId, CancellationToken cancellationToken);
    Task<CoachThreadResponse> RenameThreadAsync(Guid userId, Guid threadId, UpdateCoachThreadRequest request, CancellationToken cancellationToken);
    Task DeleteThreadAsync(Guid userId, Guid threadId, CancellationToken cancellationToken);
    Task<CoachQueuedExchangeResponse> SendMessageAsync(Guid userId, Guid threadId, SendCoachMessageRequest request, CancellationToken cancellationToken);
    Task<CoachMessageHistoryResponse> GetMessagesAsync(Guid userId, Guid threadId, CoachMessageHistoryFilter filter, CancellationToken cancellationToken);
    Task<CoachMessageResponse> GetMessageAsync(Guid userId, Guid threadId, Guid messageId, CancellationToken cancellationToken);
    Task<CoachMessageResponse> RetryMessageAsync(Guid userId, Guid threadId, Guid messageId, CancellationToken cancellationToken);
    Task<DailyCoachBriefingResponse> QueueDailyBriefingAsync(Guid userId, CancellationToken cancellationToken);
    Task<DailyCoachBriefingResponse?> GetTodayDailyBriefingAsync(Guid userId, CancellationToken cancellationToken);
    Task<DailyCoachBriefingResponse> GetDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken);
    Task<DailyCoachBriefingResponse> RetryDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken);
    Task<DailyCoachBriefingResponse> RegenerateDailyBriefingAsync(Guid userId, Guid briefingId, CancellationToken cancellationToken);
}

public sealed class CoachInteractionService : ICoachInteractionService
{
    private readonly FitspireDbContext _context;
    private readonly ICoachInteractionQueueService _queueService;
    private readonly ICoachUserTimeZoneService _timeZoneService;
    private readonly ICoachInteractionResponseFactory _responseFactory;
    private readonly IMapper _mapper;

    public CoachInteractionService(FitspireDbContext context, ICoachInteractionQueueService queueService,
        ICoachUserTimeZoneService timeZoneService, ICoachInteractionResponseFactory responseFactory, IMapper mapper)
    {
        _context = context;
        _queueService = queueService;
        _timeZoneService = timeZoneService;
        _responseFactory = responseFactory;
        _mapper = mapper;
    }

    public async Task<CoachThreadResponse> CreateThreadAsync(Guid userId, CreateCoachThreadRequest request,
        CancellationToken cancellationToken)
    {
        var thread = CoachThread.Create(Guid.NewGuid(), userId, request.Title, DateTime.UtcNow);
        _context.CoachThreads.Add(thread);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CoachThreadResponse>(thread);
    }

    public async Task<CoachThreadPageResponse> GetThreadsAsync(Guid userId, CoachThreadHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _context.CoachThreads.AsNoTracking().Where(thread => thread.UserId == userId)
            .OrderByDescending(thread => thread.LastActivityAt);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(cancellationToken);
        return new CoachThreadPageResponse(_mapper.Map<IReadOnlyList<CoachThreadListItemResponse>>(items), filter.Page,
            filter.PageSize, total);
    }

    public async Task<CoachThreadResponse> GetThreadAsync(Guid userId, Guid threadId, CancellationToken cancellationToken) =>
        _mapper.Map<CoachThreadResponse>(await FindThreadAsync(userId, threadId, false, cancellationToken));

    public async Task<CoachThreadResponse> RenameThreadAsync(Guid userId, Guid threadId, UpdateCoachThreadRequest request,
        CancellationToken cancellationToken)
    {
        var thread = await FindThreadAsync(userId, threadId, true, cancellationToken);
        thread.Rename(request.Title, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CoachThreadResponse>(thread);
    }

    public async Task DeleteThreadAsync(Guid userId, Guid threadId, CancellationToken cancellationToken)
    {
        var thread = await FindThreadAsync(userId, threadId, true, cancellationToken);
        _context.CoachThreads.Remove(thread);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CoachQueuedExchangeResponse> SendMessageAsync(Guid userId, Guid threadId, SendCoachMessageRequest request,
        CancellationToken cancellationToken)
    {
        var queued = await _queueService.QueueQuestionAsync(userId, threadId, request.ClientRequestId, request.Content, cancellationToken);
        var userMessage = await FindMessageAsync(userId, threadId, queued.UserMessageId, cancellationToken);
        var assistantMessage = await FindMessageAsync(userId, threadId, queued.AssistantMessageId, cancellationToken);
        return new CoachQueuedExchangeResponse(_responseFactory.CreateMessage(userMessage),
            _responseFactory.CreateMessage(assistantMessage), queued.Accepted);
    }

    public async Task<CoachMessageHistoryResponse> GetMessagesAsync(Guid userId, Guid threadId, CoachMessageHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        await EnsureThreadExistsAsync(userId, threadId, cancellationToken);
        var query = _context.CoachMessages.AsNoTracking().Where(message => message.UserId == userId && message.ThreadId == threadId &&
            (!filter.BeforeSequence.HasValue || message.SequenceNumber < filter.BeforeSequence.Value)).OrderByDescending(message => message.SequenceNumber);
        var page = await query.Take(filter.PageSize + 1).ToListAsync(cancellationToken);
        var hasMore = page.Count > filter.PageSize;
        var items = page.Take(filter.PageSize).OrderBy(message => message.SequenceNumber).Select(_responseFactory.CreateMessage).ToList();
        return new CoachMessageHistoryResponse(items, hasMore && items.Count > 0 ? items[0].SequenceNumber : null);
    }

    public async Task<CoachMessageResponse> GetMessageAsync(Guid userId, Guid threadId, Guid messageId,
        CancellationToken cancellationToken) => _responseFactory.CreateMessage(await FindMessageAsync(userId, threadId, messageId, cancellationToken));

    public async Task<CoachMessageResponse> RetryMessageAsync(Guid userId, Guid threadId, Guid messageId,
        CancellationToken cancellationToken)
    {
        await _queueService.RetryMessageAsync(userId, threadId, messageId, cancellationToken);
        return await GetMessageAsync(userId, threadId, messageId, cancellationToken);
    }

    public async Task<DailyCoachBriefingResponse> QueueDailyBriefingAsync(Guid userId, CancellationToken cancellationToken)
    {
        var queued = await _queueService.QueueDailyBriefingAsync(userId, cancellationToken);
        return await GetDailyBriefingAsync(userId, queued.BriefingId, cancellationToken);
    }

    public async Task<DailyCoachBriefingResponse?> GetTodayDailyBriefingAsync(Guid userId, CancellationToken cancellationToken)
    {
        var timeZoneId = await _timeZoneService.GetAsync(userId, cancellationToken);
        var localDate = CoachLocalDate.Resolve(timeZoneId, DateTime.UtcNow);
        var briefing = await _context.DailyCoachBriefings.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.UserId == userId &&
            candidate.LocalDate == localDate, cancellationToken);
        return briefing is null ? null : _responseFactory.CreateDailyBriefing(briefing);
    }

    public async Task<DailyCoachBriefingResponse> GetDailyBriefingAsync(Guid userId, Guid briefingId,
        CancellationToken cancellationToken)
    {
        var briefing = await _context.DailyCoachBriefings.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == briefingId &&
            candidate.UserId == userId, cancellationToken) ?? throw new NotFoundException("Daily coach briefing was not found.");
        return _responseFactory.CreateDailyBriefing(briefing);
    }

    public async Task<DailyCoachBriefingResponse> RetryDailyBriefingAsync(Guid userId, Guid briefingId,
        CancellationToken cancellationToken)
    {
        await _queueService.RetryDailyBriefingAsync(userId, briefingId, cancellationToken);
        return await GetDailyBriefingAsync(userId, briefingId, cancellationToken);
    }

    public async Task<DailyCoachBriefingResponse> RegenerateDailyBriefingAsync(Guid userId, Guid briefingId,
        CancellationToken cancellationToken)
    {
        await _queueService.RegenerateDailyBriefingAsync(userId, briefingId, cancellationToken);
        return await GetDailyBriefingAsync(userId, briefingId, cancellationToken);
    }

    private async Task<CoachThread> FindThreadAsync(Guid userId, Guid threadId, bool tracked, CancellationToken cancellationToken)
    {
        var query = tracked ? _context.CoachThreads : _context.CoachThreads.AsNoTracking();
        return await query.FirstOrDefaultAsync(thread => thread.Id == threadId && thread.UserId == userId, cancellationToken)
               ?? throw new NotFoundException("Coach thread was not found.");
    }

    private async Task EnsureThreadExistsAsync(Guid userId, Guid threadId, CancellationToken cancellationToken)
    {
        if (!await _context.CoachThreads.AsNoTracking().AnyAsync(thread => thread.Id == threadId && thread.UserId == userId, cancellationToken))
            throw new NotFoundException("Coach thread was not found.");
    }

    private async Task<CoachMessage> FindMessageAsync(Guid userId, Guid threadId, Guid messageId,
        CancellationToken cancellationToken) => await _context.CoachMessages.AsNoTracking().FirstOrDefaultAsync(message =>
        message.Id == messageId && message.ThreadId == threadId && message.UserId == userId, cancellationToken)
        ?? throw new NotFoundException("Coach message was not found.");
}
