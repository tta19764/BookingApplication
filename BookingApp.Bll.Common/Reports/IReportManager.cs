using BookingApp.Bll.Common.Reports.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.Reports;

/// <summary>
/// Defines business reporting operations.
/// </summary>
public interface IReportManager
{
    Task<Result<BookingSummaryModel>> GetBookingSummaryAsync(CancellationToken cancellationToken);
}
