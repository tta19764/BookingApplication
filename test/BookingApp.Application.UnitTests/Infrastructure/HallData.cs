using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;

namespace BookingApp.Application.UnitTests.Infrastructure;

internal static class HallData
{
    public static ConferenceHall Create(
        Guid? id = null,
        int capacity = 50,
        decimal hourlyRate = 2000m,
        List<Amenity>? amenities = null)
    {
        return new ConferenceHall(
            id ?? Guid.NewGuid(),
            new Name("Test Hall"),
            new Capacity(capacity),
            new Money(hourlyRate, Currency.Uah),
            amenities ?? [Amenity.Projector, Amenity.WiFi, Amenity.SoundSystem]);
    }
}
