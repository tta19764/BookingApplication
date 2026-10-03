using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Common.Bookings.Models;

public sealed record CreateBookingModel(
    Guid HallId,
    Guid UserId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyCollection<Amenity> Amenities);
