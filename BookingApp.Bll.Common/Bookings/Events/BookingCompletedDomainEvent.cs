using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Common.Bookings.Events;

/// <summary>
/// Raised when a reserved booking is completed.
/// </summary>
public record BookingCompletedDomainEvent(Guid BookingId) : IDomainEvent;
