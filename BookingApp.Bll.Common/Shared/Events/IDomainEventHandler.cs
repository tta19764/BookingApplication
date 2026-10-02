namespace BookingApp.Bll.Common.Shared.Events;

/// <summary>
/// Handles one domain-event type.
/// </summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
