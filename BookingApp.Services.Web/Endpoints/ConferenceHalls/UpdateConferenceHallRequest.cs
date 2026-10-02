using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Services.Web.Endpoints.ConferenceHalls;

/// <summary>
/// Request body for replacing editable conference hall details.
/// </summary>
public sealed record UpdateConferenceHallRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<Amenity> Amenities);
