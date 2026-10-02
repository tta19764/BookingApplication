using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.ConferenceHalls.AddHall;

/// <summary>
/// Request for creating a conference hall with its capacity, hourly rate, and supported amenities.
/// </summary>
public record AddHallRequest(
    string Name,
    int Capacity,
    decimal HourlyRate,
    string CurrencyCode,
    IReadOnlyCollection<Amenity> Amenities) : IManagerRequest<Result<Guid>>;
