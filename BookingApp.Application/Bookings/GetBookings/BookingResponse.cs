namespace BookingApp.Application.Bookings.GetBookings;

/// <summary>
/// Booking read model used by paginated booking queries.
/// </summary>
public sealed record BookingResponse(
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
