using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Application.ConferenceHalls.GetHall;

namespace BookingApp.Application.ConferenceHalls.GetHalls;

/// <summary>
/// Query for reading one page of conference halls.
/// </summary>
public sealed record GetHallsQuery(int Page, int PageSize) : IQuery<IReadOnlyCollection<HallResponse>>;
