using BookingApp.Bll.Abstractions.Messaging;

namespace BookingApp.Bll.ConferenceHalls.RemoveHall;

/// <summary>
/// Request for removing a conference hall by identifier.
/// </summary>
public record RemoveHallRequest(Guid HallId) : IManagerRequest;
