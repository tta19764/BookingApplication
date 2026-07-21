using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

/// <summary>
/// Raised when a reserved booking is rejected.
/// </summary>
public record BookingRejectedDomainEvent(Guid BookingId) : IDomainEvent;
