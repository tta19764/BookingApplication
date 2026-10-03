namespace BookingApp.Bll.Common.ConferenceHalls.Models;

public sealed record UpdateHallModel(
    Guid HallId,
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<Amenity> Amenities);
