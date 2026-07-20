using BookingApp.Domain.Shared;

namespace BookingApp.Domain.ConferenceHalls;

public enum Amenity
{
    Projector = 1,
    WiFi = 2,
    SoundSystem = 3
}

public static class AmenityExtensions
{
    public static Money GetPrice(this Amenity amenity, Currency currency)
    {
        if (Currency.All.All(c => c != currency))
        {
            throw new NotSupportedException($"Currency '{currency}' is not supported.");
        }

        if (currency == Currency.Uah)
        {
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