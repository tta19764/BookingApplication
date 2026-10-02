using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Common.Bookings.Events;

/// <summary>
/// Raised when a reserved booking is rejected.
/// </summary>
public record BookingRejectedDomainEvent(Guid BookingId) : IDomainEvent;
