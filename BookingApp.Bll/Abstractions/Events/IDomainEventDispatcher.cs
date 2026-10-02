using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Abstractions.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
