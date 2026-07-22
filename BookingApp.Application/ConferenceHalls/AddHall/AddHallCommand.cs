using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.AddHall;

/// <summary>
/// Command for creating a conference hall with its capacity, hourly rate, and supported amenities.
/// </summary>
public record AddHallCommand(
    string Name,
    int Capacity,
    decimal HourlyRate,
    string CurrencyCode,
    IReadOnlyCollection<Amenity> Amenities) : ICommand<Guid>;