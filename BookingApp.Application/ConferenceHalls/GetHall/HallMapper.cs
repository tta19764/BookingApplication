using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.GetHall;

/// <summary>
/// Maps conference hall domain entities to application read models.
/// </summary>
internal static class HallMapper
{
    /// <summary>
    /// Converts a conference hall into a response model with amenity prices.
    /// </summary>
    internal static HallResponse ToResponse(ConferenceHall hall)
    {
        return new HallResponse(
            hall.Id,
            hall.Name.Value,
            hall.Seats.Value,
            hall.Price.Amount,
            hall.Price.Currency.Code,
            hall.Amenities
                .Select(amenity =>
                {
                    var price = amenity.GetPrice(hall.Price.Currency);

                    return new AmenityResponse(
                        amenity,
                        GetDisplayName(amenity),
                        price.Amount,
                        price.Currency.Code);
                })
                .ToList());
    }

    private static string GetDisplayName(Amenity amenity)
    {
        return amenity switch
        {
            Amenity.Projector => "Projector",
            Amenity.WiFi => "Wi-Fi",
            Amenity.SoundSystem => "Sound system",
            _ => amenity.ToString()
        };
    }
}
