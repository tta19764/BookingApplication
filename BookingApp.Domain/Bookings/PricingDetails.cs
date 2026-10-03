using BookingApp.Domain.Shared;

namespace BookingApp.Domain.Bookings;

/// <summary>
/// Detailed price breakdown for a hall booking.
/// </summary>
public record PricingDetails(
    Money PriceForPeriod,
    Money AmenitiesUpCharge,
    Money TotalPrice);