using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.UpdateHall;

/// <summary>
/// Command for replacing editable conference hall details.
/// </summary>
public record UpdateHallCommand(
    Guid HallId,
    string Name,
    int Capacity,
    decimal HourlyRate,
    IReadOnlyCollection<Amenity> Amenities) : ICommand;