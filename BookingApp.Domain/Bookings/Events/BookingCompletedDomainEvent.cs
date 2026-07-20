using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

public record BookingCompletedDomainEvent(Guid BookingId) : IDomainEvent;