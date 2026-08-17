using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Goal.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Shared.Service;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Features;

public record CreateGoalCommand(Guid UserId, Guid GoalTypeId, double TargetValue, string Schedule, DateTime? Deadline,
    bool IsPublic = false,
    string? SelectedWorkoutType = null, Guid? SelectedExerciseId = null, DateTime? StartDate = null) : IRequest<Guid>;

public class CreateGoalHandler : IRequestHandler<CreateGoalCommand, Guid>
{
    private readonly IGoalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FitspireDbContext _context;
    private readonly IGoalTemplatePolicy _templatePolicy;
    private readonly IGoalTransactionService _transactions;
    private readonly IUserLocalDateResolver _localDateResolver;

    public CreateGoalHandler(IGoalRepository repository, IUnitOfWork unitOfWork, FitspireDbContext context,
        IGoalTemplatePolicy templatePolicy, IGoalTransactionService transactions, IUserLocalDateResolver localDateResolver)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
        _templatePolicy = templatePolicy;
        _transactions = transactions;
        _localDateResolver = localDateResolver;
    }

    public async Task<Guid> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        return await _transactions.ExecuteAsync(token => CreateAsync(request, token), cancellationToken);
    }

    private async Task<Guid> CreateAsync(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var goalType = await _repository.GetGoalTypeByIdAsync(request.GoalTypeId, cancellationToken)
            ?? throw new NotFoundException($"Goal type {request.GoalTypeId} not found.");

        // Resolve the user's bare calendar-date input to UTC via their saved timezone preference —
        // DateTime.ToUniversalTime() on an unspecified-kind value would silently use the server's
        // own local timezone instead, which is what GoalTemplatePolicy used to do.
        var deadlineUtc = request.Deadline.HasValue
            ? await _localDateResolver.ResolveUtcAsync(request.UserId, request.Deadline.Value, cancellationToken)
            : (DateTime?)null;
        var startDateUtc = request.StartDate.HasValue
            ? await _localDateResolver.ResolveUtcAsync(request.UserId, request.StartDate.Value, cancellationToken)
            : (DateTime?)null;

        var rules = _templatePolicy.Resolve(goalType, request.Schedule, deadlineUtc, request.SelectedWorkoutType,
            request.SelectedExerciseId, startDateUtc);
        await EnsureMetricAndExerciseAsync(goalType, rules.SelectedExerciseId, cancellationToken);

        var definitionKey = GoalDefinitionKeyFactory.Create(goalType, request.Schedule.Trim().ToLowerInvariant(), rules.SelectedWorkoutType, rules.SelectedExerciseId);
        var duplicate = await _context.Goals.AnyAsync(goal => goal.UserId == request.UserId && goal.Status == Domain.Enums.GoalStatus.Active && goal.DefinitionKey == definitionKey, cancellationToken);
        if (duplicate)
            throw new DomainException("An active goal with the same template, filter, and schedule already exists.");

        var timeZoneId = await _context.UserPreferences.Where(preference => preference.UserId == request.UserId)
            .Select(preference => preference.TimeZoneId).FirstOrDefaultAsync(cancellationToken) ?? "Central European Standard Time";
        var (start, end) = GetInitialPeriod(rules, timeZoneId);
        var goal = new UserGoal(Guid.NewGuid(), request.UserId, goalType.Id, request.TargetValue, goalType.DefaultUnit,
            rules.StartDate, rules.Deadline, rules.IsRecurring, rules.RecurrencePattern, request.IsPublic);
        goal.SetTemplateParameters(timeZoneId, rules.SelectedWorkoutType, rules.SelectedExerciseId);
        goal.SetDefinitionKey(definitionKey);

        await _repository.AddAsync(goal, cancellationToken);
        await _context.GoalPeriods.AddAsync(new GoalPeriod(goal.Id, start, end, request.TargetValue), cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsActiveDefinitionConflict(exception))
        {
            throw new DomainException("An active goal with the same template, filter, and schedule already exists.", exception);
        }
        return goal.Id;
    }

    private async Task EnsureMetricAndExerciseAsync(GoalType goalType, Guid? exerciseId, CancellationToken cancellationToken)
    {
        var metric = await _context.MetricDefinitions.FindAsync([goalType.MetricCode!], cancellationToken);
        if (metric is null || !metric.IsActive || !metric.IsGoalSupported)
            throw new DomainException("This goal template does not use a supported metric.");
        if (exerciseId.HasValue && !await _context.Exercises.AnyAsync(exercise => exercise.Id == exerciseId, cancellationToken))
            throw new NotFoundException("Exercise not found.");
    }

    private static (DateTime Start, DateTime End) GetInitialPeriod(GoalCreationRules rules, string timeZoneId)
    {
        if (!rules.IsRecurring)
            return (rules.StartDate, rules.Deadline!.Value);
        return GoalPeriodBoundaries.Current(rules.RecurrencePattern, timeZoneId, rules.StartDate);
    }

    private static bool IsActiveDefinitionConflict(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains("UX_UserGoal_ActiveDefinition", StringComparison.OrdinalIgnoreCase) == true;
}
