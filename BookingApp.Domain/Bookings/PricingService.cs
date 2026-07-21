using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;

namespace BookingApp.Domain.Bookings;

/// <summary>
/// Calculates booking prices from hall hourly rate, time-based modifiers, and selected amenities.
/// </summary>
public sealed class PricingService
{
    private static readonly int[] Boundaries = [6, 9, 12, 14, 18, 23];

    /// <summary>
    /// Calculates the full booking price and validates that all selected amenities are supported by the hall.
    /// </summary>
    public PricingDetails CalculatePrice(
        ConferenceHall hall,
        DateRange period,
        IEnumerable<Amenity>? amenities = null)
    {
        foreach (var amenity in amenities ?? [])
        {
            if (!hall.SupportsAmenity(amenity))
            {
                throw new ArgumentException(
                    $"Hall '{hall.Name}' does not support '{amenity}'.");
            }
        }
        
        var priceForPeriod = CalculatePriceForPeriod(hall.Price, period);

        var amenitiesUpCharge = CalculateAmenitiesPrice(
            hall.Price.Currency,
            amenities ?? Enumerable.Empty<Amenity>());

        var totalPrice = priceForPeriod + amenitiesUpCharge;

        return new PricingDetails(
            priceForPeriod,
            amenitiesUpCharge,
            totalPrice);
    }

    private static Money CalculatePriceForPeriod(
        Money hourlyRate,
        DateRange period)
    {
        var total = Money.Zero(hourlyRate.Currency);

        var current = period.Start;

        while (current < period.End)
        {
            var nextBoundary = GetNextBoundary(current);
            var intervalEnd = nextBoundary < period.End
                ? nextBoundary
                : period.End;

            var hours = (decimal)(intervalEnd - current).TotalHours;

            total += hourlyRate with { Amount = hours * hourlyRate.Amount * GetPriceModifier(current) };

            current = intervalEnd;
        }

        return total;
    }

    private static Money CalculateAmenitiesPrice(
        Currency currency,
        IEnumerable<Amenity> amenities)
    {
        return amenities
            .Select(a => a.GetPrice(currency))
            .Aggregate(
                Money.Zero(currency),
                (sum, price) => sum + price);
    }

    private static decimal GetPriceModifier(DateTime time)
    {
        return time.Hour switch
        {
            >= 6 and < 9 => 0.90m,
            >= 9 and < 12 => 1.00m,
            >= 12 and < 14 => 1.15m,
            >= 14 and < 18 => 1.00m,
            >= 18 and < 23 => 0.80m,
            _ => throw new InvalidOperationException(
                "Bookings are allowed only between 06:00 and 23:00.")
        };
    }

    private static DateTime GetNextBoundary(DateTime time)
    {
        foreach (var boundary in Boundaries)
        {
            var boundaryTime = time.Date.AddHours(boundary);

            if (boundaryTime > time)
            {
                return boundaryTime;
            }
        }

        return time.Date.AddDays(1).AddHours(6);
    }
}