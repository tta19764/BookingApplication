using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Services.Web.Endpoints.ConferenceHalls;

/// <summary>
/// Request body for creating a conference hall.
/// </summary>
public sealed record CreateConferenceHallRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    string CurrencyCode,
    IReadOnlyCollection<Amenity> Amenities);
