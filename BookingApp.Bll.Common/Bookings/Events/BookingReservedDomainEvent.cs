using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Common.Bookings.Events;

/// <summary>
/// Raised when a booking is reserved.
/// </summary>
public record BookingReservedDomainEvent(Guid BookingId) : IDomainEvent;
