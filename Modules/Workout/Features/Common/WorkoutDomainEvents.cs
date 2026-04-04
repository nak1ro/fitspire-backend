using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

internal static class WorkoutDomainEvents
{
    public static IReadOnlyCollection<WorkoutCompletedEvent> PullCompletionEvents(UserWorkout workout)
    {
        var events = workout.DomainEvents
            .OfType<WorkoutCompletedEvent>()
            .ToList();

        workout.ClearDomainEvents();
        return events;
    }

    public static async Task PublishAsync(
        IPublisher publisher,
        IEnumerable<WorkoutCompletedEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
