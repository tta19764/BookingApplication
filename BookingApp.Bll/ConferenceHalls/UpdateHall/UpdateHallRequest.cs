using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.ConferenceHalls.UpdateHall;

/// <summary>
/// Request for replacing editable conference hall details.
/// </summary>
public record UpdateHallRequest(
    Guid HallId,
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<Amenity> Amenities) : IManagerRequest;