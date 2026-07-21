using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Application.ConferenceHalls.GetHall;

namespace BookingApp.Application.ConferenceHalls.GetAvailableHalls;

public record GetAvailableHallsQuery() : IQuery<IEnumerable<HallResponse>>;