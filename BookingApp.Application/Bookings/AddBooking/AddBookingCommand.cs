using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.Bookings.AddBooking;

/// <summary>
/// Command for reserving a hall for a time period with selected amenities.
/// </summary>
public record AddBookingCommand(
    Guid HallId,
    Guid UserId,
    DateTime Start,
    DateTime End,
    IReadOnlyCollection<Amenity> Amenities) : ICommand<BookingConfirmationResponse>;
