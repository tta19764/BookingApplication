using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Common.Bookings;

/// <summary>
/// Calculates a booking price according to the configured business tariff rules.
/// </summary>
public interface IPricingManager
{
    /// <summary>Calculates rental and amenity charges for the requested period.</summary>
    /// <param name="hall">Conference hall details.</param>
    /// <param name="period">Rental period used to calculate the price.</param>
    /// <param name="amenities">Selected amenities; null means none.</param>
    /// <returns>The rental, amenity, and total prices.</returns>
    PricingDetails CalculatePrice(
        ConferenceHall hall,
        DateRange period,
        IEnumerable<Amenity>? amenities = null);
}
