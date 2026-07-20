using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

public record BookingReservedDomainEvent(Guid BookingId) : IDomainEvent;