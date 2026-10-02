using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request body for replacing editable conference hall details.
/// </summary>
public sealed record UpdateConferenceHallRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<Amenity> Amenities);
