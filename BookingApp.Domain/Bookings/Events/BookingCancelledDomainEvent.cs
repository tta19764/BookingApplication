using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

public record BookingCancelledDomainEvent(Guid BookingId) : IDomainEvent;