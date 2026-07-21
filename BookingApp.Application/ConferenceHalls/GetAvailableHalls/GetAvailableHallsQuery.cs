using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Application.ConferenceHalls.GetHall;

namespace BookingApp.Application.ConferenceHalls.GetAvailableHalls;

/// <summary>
/// Query for finding halls available for a requested period and minimum capacity.
/// </summary>
public record GetAvailableHallsQuery(
    DateTime Start,
    DateTime End,
    int Capacity) : IQuery<IEnumerable<HallResponse>>;
