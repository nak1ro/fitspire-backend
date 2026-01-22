using MediatR;

namespace backend.Modules.Shared.Domain;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
