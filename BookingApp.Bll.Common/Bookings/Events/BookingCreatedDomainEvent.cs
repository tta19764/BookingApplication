using BookingApp.Bll.Common.Shared.Events;

namespace BookingApp.Bll.Common.Bookings.Events;

/// <summary>
/// Business fact emitted after a booking has been persisted successfully.
/// </summary>
public sealed record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid ConferenceHallId,
    Guid UserId,
    decimal TotalPrice,
    string Currency,
    DateTime OccurredOnUtc) : IDomainEvent;
