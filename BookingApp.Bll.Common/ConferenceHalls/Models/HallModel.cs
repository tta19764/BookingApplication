namespace BookingApp.Bll.Common.ConferenceHalls.Models;

/// <summary>
/// Amenity data exposed to API consumers, including its fixed price.
/// </summary>
public sealed record AmenityModel(
    Amenity Type,
    string Name,
    decimal Price,
    string Currency);

/// <summary>
/// Conference hall read model used by hall queries.
/// </summary>
public sealed record HallModel(
    Guid Id,
    string Name,
    int Capacity,
    decimal HourlyRate,
    string Currency,
    IReadOnlyCollection<AmenityModel> Amenities);