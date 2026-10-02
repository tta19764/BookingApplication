using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Shared.Models;

namespace BookingApp.Bll.Common.Bookings;

/// <summary>
/// Defines booking operations exposed by the business logic layer.
/// </summary>
public interface IBookingManager
{
    Task<Result<IReadOnlyCollection<BookingModel>>> GetBookingsAsync(
        PaginationModel pagination,
        CancellationToken cancellationToken);

    Task<Result<BookingConfirmationModel>> AddBookingAsync(
        CreateBookingModel model,
        CancellationToken cancellationToken);
}
