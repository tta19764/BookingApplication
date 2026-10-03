using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.ConferenceHalls.RemoveHall;

/// <summary>
/// Command for removing a conference hall by identifier.
/// </summary>
public record RemoveHallCommand(Guid HallId) : ICommand;
