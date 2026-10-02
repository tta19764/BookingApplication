using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Models;

namespace BookingApp.Bll.Common.Reports;

/// <summary>
/// Defines business reporting operations.
/// </summary>
public interface IReportManager
{
    Task<Result<BookingSummaryModel>> GetBookingSummaryAsync(CancellationToken cancellationToken);
}
