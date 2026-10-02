using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Bookings.AddBooking;
using BookingApp.Bll.Bookings.GetBookings;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Models;

namespace BookingApp.Bll.Bookings;

/// <summary>
/// Coordinates booking use cases through the BLL validation pipeline.
/// </summary>
internal sealed class BookingManager(IManagerDispatcher dispatcher) : IBookingManager
{
    public Task<Result<IReadOnlyCollection<BookingModel>>> GetBookingsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        dispatcher.Send(new GetBookingsRequest(page, pageSize), cancellationToken);

    public Task<Result<BookingConfirmationModel>> AddBookingAsync(
        Guid hallId,
        Guid userId,
        DateOnly date,
        string startTime,
        string endTime,
        IReadOnlyCollection<Amenity> amenities,
        CancellationToken cancellationToken) =>
        dispatcher.Send(
            new AddBookingRequest(hallId, userId, date, startTime, endTime, amenities),
            cancellationToken);
}
