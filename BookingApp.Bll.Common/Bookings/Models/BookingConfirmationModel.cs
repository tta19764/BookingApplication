namespace BookingApp.Bll.Common.Models;

/// <summary>
/// Booking confirmation returned after a successful reservation, including the price breakdown.
/// </summary>
public sealed record BookingConfirmationModel(
    Guid BookingId,
    Guid HallId,
    DateTime Start,
    DateTime End,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency);
