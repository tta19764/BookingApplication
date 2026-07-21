using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.ConferenceHalls.GetHall;

/// <summary>
/// Query for retrieving one conference hall by identifier.
/// </summary>
public record GetHallQuery(Guid HallId) : IQuery<HallResponse>;