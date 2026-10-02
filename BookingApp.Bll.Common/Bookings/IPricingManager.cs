using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Common.Bookings;

/// <summary>
/// Calculates a booking price according to the configured business tariff rules.
/// </summary>
public interface IPricingManager
{
    PricingDetails CalculatePrice(
        ConferenceHall hall,
        DateRange period,
        IEnumerable<Amenity>? amenities = null);
}
