using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>
/// Detailed price breakdown for a hall booking.
/// </summary>
/// <param name="PriceForPeriod">Hall rental cost for the requested period.</param>
/// <param name="AmenitiesUpCharge">Total charge for selected amenities.</param>
/// <param name="TotalPrice">Combined rental and amenity cost.</param>
public record PricingDetails(
    Money PriceForPeriod,
    Money AmenitiesUpCharge,
    Money TotalPrice)
{
    /// <summary>Hall rental cost for the requested period.</summary>
    public Money PriceForPeriod { get; init; } = PriceForPeriod;

    /// <summary>Total charge for selected amenities.</summary>
    public Money AmenitiesUpCharge { get; init; } = AmenitiesUpCharge;

    /// <summary>Combined rental and amenity cost.</summary>
    public Money TotalPrice { get; init; } = TotalPrice;
}