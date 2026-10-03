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
        // Pricing is allowed only for amenities that belong to the selected hall.
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
        ValidatePricingPeriod(period);

        var total = Money.Zero(hourlyRate.Currency);

        var current = period.Start;

        while (current < period.End)
        {
            // Split the booking by pricing boundaries so each slice receives the correct modifier.
            var nextBoundary = GetNextBoundary(current);
            var intervalEnd = nextBoundary < period.End
                ? nextBoundary
                : period.End;

            var hours = (decimal)(intervalEnd - current).Ticks / TimeSpan.TicksPerHour;

            // The modifier is based on the slice start because slices never cross a pricing boundary.
            total += hourlyRate with { Amount = hours * hourlyRate.Amount * GetPriceModifier(current) };

            current = intervalEnd;
        }

        return total;
    }

    private static void ValidatePricingPeriod(DateRange period)
    {
        if (period.Start.Date != period.End.Date)
        {
            throw new InvalidOperationException("Booking period must be within one calendar day.");
        }

        if (!IsMinutePrecision(period.Start) || !IsMinutePrecision(period.End))
        {
            throw new InvalidOperationException("Bookings must start and end at minute precision.");
        }

        if (period.Start.Hour < 6 || period.End.Hour > 23)
        {
            throw new InvalidOperationException("Bookings are allowed only between 06:00 and 23:00.");
        }
    }

    private static bool IsMinutePrecision(DateTime dateTime)
    {
        return dateTime is { Second: 0, Millisecond: 0, Microsecond: 0 };
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
        // Boundaries are evaluated within the current day first.
        foreach (var boundary in Boundaries)
        {
            var boundaryTime = time.Date.AddHours(boundary);

            if (boundaryTime > time)
            {
                return boundaryTime;
            }
        }

        // Cross-day bookings are currently rejected before pricing reaches this point.
        // If overnight bookings are added later, return the next day's opening boundary here.
        throw new InvalidOperationException("No pricing boundary was found for the current day.");
    }
}
