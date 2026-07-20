using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

public record BookingRejectedDomainEvent(Guid BookingId) : IDomainEvent;