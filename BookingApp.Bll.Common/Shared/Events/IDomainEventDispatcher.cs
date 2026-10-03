namespace BookingApp.Bll.Common.Shared.Events;

/// <summary>
/// Dispatches domain events without coupling BLL managers to concrete handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
