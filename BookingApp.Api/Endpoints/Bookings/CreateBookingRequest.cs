using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Api.Endpoints.Bookings;

/// <summary>
/// Request body for creating a booking for the seeded user.
/// </summary>
public sealed record CreateBookingRequest(
    Guid HallId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    IReadOnlyCollection<Amenity> Amenities);
