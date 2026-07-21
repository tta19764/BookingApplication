using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.GetHall;

/// <summary>
/// Amenity data exposed to API consumers, including its fixed price.
/// </summary>
public sealed record AmenityResponse(
    Amenity Type,
    string Name,
    decimal Price,
    string Currency);

/// <summary>
/// Conference hall read model used by hall queries.
/// </summary>
public sealed record HallResponse(
    Guid Id,
    string Name,
    int Capacity,
    decimal HourlyRate,
    string Currency,
    IReadOnlyCollection<AmenityResponse> Amenities);