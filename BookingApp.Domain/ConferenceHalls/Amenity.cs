using BookingApp.Domain.Shared;

namespace BookingApp.Domain.ConferenceHalls;

/// <summary>
/// Fixed catalog of optional services that can be attached to a hall booking.
/// </summary>
public enum Amenity
{
    /// <summary>
    /// Projector rental.
    /// </summary>
    Projector = 1,

    /// <summary>
    /// Wi-Fi access.
    /// </summary>
    WiFi = 2,

    /// <summary>
    /// Sound system rental.
    /// </summary>
    SoundSystem = 3
}

/// <summary>
/// Provides pricing behavior for amenities.
/// </summary>
public static class AmenityExtensions
{
    /// <summary>
    /// Gets the fixed amenity price in the requested supported currency.
    /// </summary>
    public static Money GetPrice(this Amenity amenity, Currency currency)
    {
        // Amenity prices are defined only for currencies explicitly supported by the domain.
        if (Currency.All.All(c => c != currency))
        {
            throw new NotSupportedException($"Currency '{currency}' is not supported.");
        }

        if (currency == Currency.Uah)
        {
            // Prices are fixed per booking, not multiplied by booking duration.
            return amenity switch
            {
                Amenity.Projector => new Money(500m, currency),
                Amenity.WiFi => new Money(300m, currency),
                Amenity.SoundSystem => new Money(700m, currency),
                _ => throw new ArgumentOutOfRangeException(nameof(amenity))
            };
        }
        
        throw new NotSupportedException($"Currency '{currency}' is not supported.");
    }
}