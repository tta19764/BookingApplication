using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.Bookings.AddBooking;

/// <summary>
/// Command for reserving a hall for a time period with selected amenities.
/// </summary>
public record AddBookingCommand(
    Guid HallId,
    Guid UserId,
    DateOnly Date,
    string StartTime,
    string EndTime,
    IReadOnlyCollection<Amenity> Amenities) : ICommand<BookingConfirmationResponse>;
