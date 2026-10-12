namespace BookingApp.Bll.Common.Shared.Events;

/// <summary>
/// Dispatches domain events without coupling BLL managers to concrete handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>Dispatches a domain event to all registered handlers.</summary>
    /// <typeparam name="TEvent">The domain event type to process.</typeparam>
    /// <param name="domainEvent">Domain event to process.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>A task completed after all registered handlers finish.</returns>
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
