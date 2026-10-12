using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.Bll.Common.Bookings;

public interface IBookingRepository
{
    /// <summary>Finds a booking by its identifier.</summary>
    /// <param name="id">Identifier of the requested booking.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The matching item, or null when it does not exist.</returns>
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a reserved booking overlaps the requested hall rental period.</summary>
    /// <param name="conferenceHallId">Identifier of the hall to check.</param>
    /// <param name="duration">Requested rental period in UTC.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>Whether an overlapping reserved booking exists.</returns>
    Task<bool> HasOverlapAsync(Guid conferenceHallId, DateRange duration, CancellationToken cancellationToken = default);

    /// <summary>Streams bookings in pages until the catalog is exhausted.</summary>
    /// <param name="pageSize">Maximum number of items per page or batch.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>An asynchronous sequence of booking pages.</returns>
    IAsyncEnumerable<IReadOnlyCollection<Booking>> ListAsync(int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Gets one page of persisted bookings.</summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Maximum number of items per page or batch.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The items in the requested page.</returns>
    Task<IReadOnlyCollection<Booking>> GetListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Gets a batch of reserved bookings whose rental periods have ended.</summary>
    /// <param name="utcNow">UTC cutoff for completing expired reservations.</param>
    /// <param name="pageSize">Maximum number of items per page or batch.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>A batch of reservations due for completion.</returns>
    Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletionAsync(DateTime utcNow, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Atomically reserves a hall if the requested period is available.</summary>
    /// <param name="booking">Booking to persist as a reservation.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>Whether the reservation was created or rejected because of a conflict.</returns>
    Task<ReservationOutcome> CreateReservationAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>Atomically completes a batch of due reservations and returns the updated count.</summary>
    /// <param name="utcNow">UTC cutoff for completing expired reservations.</param>
    /// <param name="pageSize">Maximum number of items per page or batch.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The number of reservations completed in this batch.</returns>
    Task<int> CompleteDueAsync(DateTime utcNow, int pageSize, CancellationToken cancellationToken = default);
}
