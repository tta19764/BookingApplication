using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.ConferenceHalls.GetHall;

public record GetHallQuery() : IQuery<Guid>, IQuery<HallResponse>;