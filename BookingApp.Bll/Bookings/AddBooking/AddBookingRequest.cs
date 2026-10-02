using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.Bookings.AddBooking;

/// <summary>
/// Request for reserving a hall for a time period with selected amenities.
/// </summary>
public record AddBookingRequest(
    Guid HallId,
    Guid UserId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    IReadOnlyCollection<Amenity> Amenities) : IManagerRequest<Result<BookingConfirmationModel>>;
