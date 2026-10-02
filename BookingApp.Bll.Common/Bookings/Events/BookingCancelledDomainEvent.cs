using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Common.Bookings.Events;

/// <summary>
/// Raised when a reserved booking is cancelled.
/// </summary>
public record BookingCancelledDomainEvent(Guid BookingId) : IDomainEvent;
