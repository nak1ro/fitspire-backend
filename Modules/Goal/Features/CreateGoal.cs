using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Goal.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Features;

public record CreateGoalCommand(Guid UserId, Guid GoalTypeId, double TargetValue, string Unit, DateTime? Deadline,
    bool IsRecurring = false, string? RecurrencePattern = null, bool IsPublic = false,
    string? SelectedWorkoutType = null, Guid? SelectedExerciseId = null) : IRequest<Guid>;

public class CreateGoalHandler : IRequestHandler<CreateGoalCommand, Guid>
{
    private readonly IGoalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FitspireDbContext _context;

    public CreateGoalHandler(IGoalRepository repository, IUnitOfWork unitOfWork, FitspireDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Guid> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var goalType = await _repository.GetGoalTypeByIdAsync(request.GoalTypeId, cancellationToken)
            ?? throw new NotFoundException($"Goal type {request.GoalTypeId} not found.");
        if (string.IsNullOrWhiteSpace(goalType.MetricCode))
            throw new DomainException("This retired goal template cannot be used. Select a supported fitness template.");
        if (goalType.ParameterKind == "Exercise" && !request.SelectedExerciseId.HasValue)
            throw new DomainException("This goal template requires an exercise.");

        var timeZoneId = await _context.UserPreferences.Where(preference => preference.UserId == request.UserId)
            .Select(preference => preference.TimeZoneId).FirstOrDefaultAsync(cancellationToken) ?? "Central European Standard Time";
        var (start, end) = GoalPeriodBoundaries.Current(request.IsRecurring ? request.RecurrencePattern : null, timeZoneId, DateTime.UtcNow);
        if (!request.IsRecurring && request.Deadline.HasValue)
            end = request.Deadline.Value.ToUniversalTime();

        var goal = new UserGoal(Guid.NewGuid(), request.UserId, request.GoalTypeId, request.TargetValue, request.Unit,
            start, end, request.IsRecurring, request.RecurrencePattern, request.IsPublic);
        goal.SetTemplateParameters(timeZoneId, request.SelectedWorkoutType, request.SelectedExerciseId);

        await _repository.AddAsync(goal, cancellationToken);
        await _context.GoalPeriods.AddAsync(new GoalPeriod(goal.Id, start, end, request.TargetValue), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return goal.Id;
    }
}
