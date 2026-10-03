using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Services.Web.Dtos;

public sealed record AmenityDto(Amenity Type, string Name, decimal Price, string Currency);

public sealed record ConferenceHallDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal HourlyRate,
    string Currency,
    IReadOnlyCollection<AmenityDto> Amenities);
