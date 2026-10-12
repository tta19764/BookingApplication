namespace BookingApp.Bll.Common.Shared.Events;

/// <summary>
/// Handles one domain-event type.
/// </summary>
/// <typeparam name="TEvent">The domain event type handled by this contract.</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>Handles the supplied domain event.</summary>
    /// <param name="domainEvent">Domain event to process.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>A task completed after the event has been handled.</returns>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
