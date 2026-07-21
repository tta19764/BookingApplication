using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings.Events;

/// <summary>
/// Raised when a reserved booking is completed.
/// </summary>
public record BookingCompletedDomainEvent(Guid BookingId) : IDomainEvent;
