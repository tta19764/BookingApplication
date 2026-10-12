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
    /// <summary>Gets a page of bookings after validating pagination.</summary>
    /// <param name="pagination">One-based page number and maximum page size.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>A page of bookings, or a validation failure.</returns>
    Task<Result<IReadOnlyCollection<BookingModel>>> GetBookingsAsync(
        PaginationModel pagination,
        CancellationToken cancellationToken);

    /// <summary>Validates and reserves a hall, calculates the price, and publishes the booking event.</summary>
    /// <param name="model">Validated input for the requested operation.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>The reservation confirmation and prices, or a business failure.</returns>
    Task<Result<BookingConfirmationModel>> AddBookingAsync(
        CreateBookingModel model,
        CancellationToken cancellationToken);
}
