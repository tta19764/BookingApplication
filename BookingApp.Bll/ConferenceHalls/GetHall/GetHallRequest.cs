using BookingApp.Bll.Abstractions.Messaging;

namespace BookingApp.Bll.ConferenceHalls.GetHall;

/// <summary>
/// Request for retrieving one conference hall by identifier.
/// </summary>
public record GetHallRequest(Guid HallId) : IManagerRequest<Result<HallResponse>>;
