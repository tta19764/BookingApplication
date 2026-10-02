using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.ConferenceHalls.GetHall;

namespace BookingApp.Bll.ConferenceHalls.GetHalls;

/// <summary>
/// Request for reading one page of conference halls.
/// </summary>
public sealed record GetHallsRequest(int Page, int PageSize) : IManagerRequest<Result<IReadOnlyCollection<HallModel>>>;
