using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.ConferenceHalls.AddHall;

public record AddHallCommand() : ICommand<Guid>;