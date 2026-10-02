namespace BookingApp.Bll.Common.Models;

/// <summary>
/// Booking read model used by paginated booking queries.
/// </summary>
public sealed record BookingModel(
    Guid Id,
    Guid HallId,
    Guid UserId,
    DateTime Start,
    DateTime End,
    string Status,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency);
