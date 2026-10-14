using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.Bll.Common.Bookings;

/// <summary>Reads rentals overlapping a half-open UTC reporting period.</summary>
public interface IBookingReportRepository
{
    /// <summary>Returns all statuses, ordered by rental start and identifier.</summary>
    /// <param name="startUtc">Inclusive UTC start of the reporting interval.</param>
    /// <param name="endUtc">Exclusive UTC end of the reporting interval.</param>
    /// <param name="cancellationToken">Cancels database access.</param>
    /// <returns>Rentals overlapping the interval with hall names and price snapshots.</returns>
    Task<IReadOnlyList<BookingReportRow>> GetAsync(DateTime startUtc, DateTime endUtc,
        CancellationToken cancellationToken = default);
}


