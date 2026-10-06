namespace BookingApp.Bll.Common.ConferenceHalls.Models;

public sealed record CreateHallModel(
    string Name,
    int Capacity,
    decimal HourlyRate,
    string CurrencyCode,
    IReadOnlyCollection<Amenity> Amenities);
