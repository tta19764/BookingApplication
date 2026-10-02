using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Abstractions.Events;

public interface IDomainEventManager<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
