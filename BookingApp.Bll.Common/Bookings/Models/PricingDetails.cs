using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>
/// Detailed price breakdown for a hall booking.
/// </summary>
public record PricingDetails(
    Money PriceForPeriod,
    Money AmenitiesUpCharge,
    Money TotalPrice);