namespace BookingApp.Application.Bookings.AddBooking;

/// <summary>
/// Booking confirmation returned after a successful reservation, including the price breakdown.
/// </summary>
public sealed record BookingConfirmationResponse(
    Guid BookingId,
    Guid HallId,
    DateTime Start,
    DateTime End,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency);
