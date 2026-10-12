using BookingApp.Bll.Common.Reports.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.Reports;

/// <summary>
/// Defines business reporting operations.
/// </summary>
public interface IReportManager
{
    /// <summary>Aggregates booking counts and revenue across halls.</summary>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>Booking counts and total revenue grouped by hall.</returns>
    Task<Result<BookingSummaryModel>> GetBookingSummaryAsync(CancellationToken cancellationToken);
}
