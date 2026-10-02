using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.Bookings;

/// <summary>
/// Defines booking operations exposed by the business logic layer.
/// </summary>
public interface IBookingManager
{
    Task<Result<IReadOnlyCollection<BookingModel>>> GetBookingsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<BookingConfirmationModel>> AddBookingAsync(
        Guid hallId,
        Guid userId,
        DateOnly date,
        string startTime,
        string endTime,
        IReadOnlyCollection<Amenity> amenities,
        CancellationToken cancellationToken);
}
