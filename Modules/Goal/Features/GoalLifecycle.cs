using backend.Data;
using backend.Modules.Goal.DTOs;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Features;

public record GetGoalPeriodsQuery(Guid UserId, Guid GoalId) : IRequest<List<GoalPeriodResponse>>;
public record UpdateGoalCommand(Guid UserId, Guid GoalId, double TargetValue, bool IsPublic) : IRequest;
public record ArchiveGoalCommand(Guid UserId, Guid GoalId) : IRequest;

public class GetGoalPeriodsHandler : IRequestHandler<GetGoalPeriodsQuery, List<GoalPeriodResponse>>
{
    private readonly FitspireDbContext _context;
    public GetGoalPeriodsHandler(FitspireDbContext context) => _context = context;
    public async Task<List<GoalPeriodResponse>> Handle(GetGoalPeriodsQuery request, CancellationToken cancellationToken) => await _context.GoalPeriods
        .Where(period => period.GoalId == request.GoalId && period.Goal.UserId == request.UserId).OrderByDescending(period => period.StartAt)
        .Select(period => new GoalPeriodResponse(period.Id, period.StartAt, period.EndAt, period.TargetValue, period.ProgressValue, period.Status, period.CompletedAt, period.FailedAt)).ToListAsync(cancellationToken);
}

public class UpdateGoalHandler : IRequestHandler<UpdateGoalCommand>
{
    private readonly FitspireDbContext _context; private readonly IUnitOfWork _unitOfWork;
    public UpdateGoalHandler(FitspireDbContext context, IUnitOfWork unitOfWork) { _context = context; _unitOfWork = unitOfWork; }
    public async Task Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals.Include(item => item.Periods).FirstOrDefaultAsync(item => item.Id == request.GoalId && item.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("Goal not found.");
        goal.UpdateTarget(request.TargetValue, request.IsPublic);
        foreach (var period in goal.Periods.Where(item => item.Status == "Active"))
            period.UpdateTarget(request.TargetValue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class ArchiveGoalHandler : IRequestHandler<ArchiveGoalCommand>
{
    private readonly FitspireDbContext _context; private readonly IUnitOfWork _unitOfWork;
    public ArchiveGoalHandler(FitspireDbContext context, IUnitOfWork unitOfWork) { _context = context; _unitOfWork = unitOfWork; }
    public async Task Handle(ArchiveGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals.FirstOrDefaultAsync(item => item.Id == request.GoalId && item.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("Goal not found.");
        goal.Archive();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
