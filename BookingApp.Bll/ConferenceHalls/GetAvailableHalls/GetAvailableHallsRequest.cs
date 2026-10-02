using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.ConferenceHalls.GetHall;

namespace BookingApp.Bll.ConferenceHalls.GetAvailableHalls;

/// <summary>
/// Request for finding halls available for a requested period and minimum capacity.
/// </summary>
public record GetAvailableHallsRequest(
    DateOnly Date,
    string StartTime,
    string EndTime,
    int Capacity) : IManagerRequest<Result<IEnumerable<HallResponse>>>;
