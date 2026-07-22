using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Api.Endpoints.Bookings;

/// <summary>
/// Request body for creating a booking for the seeded user.
/// </summary>
public sealed record CreateBookingRequest(
    Guid HallId,
    DateTime Start,
    DateTime End,
    IReadOnlyCollection<Amenity> Amenities);
