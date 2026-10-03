using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

/// <summary>
/// Raised when a reserved booking is cancelled.
/// </summary>
public record BookingCancelledDomainEvent(Guid BookingId) : IDomainEvent;
