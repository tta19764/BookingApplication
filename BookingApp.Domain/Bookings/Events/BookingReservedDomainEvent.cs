using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

/// <summary>
/// Raised when a booking is reserved.
/// </summary>
public record BookingReservedDomainEvent(Guid BookingId) : IDomainEvent;
