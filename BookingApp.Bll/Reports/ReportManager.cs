using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Common.Reports;
using BookingApp.Bll.Reports.GetBookingSummary;

namespace BookingApp.Bll.Reports;

/// <summary>
/// Coordinates business reporting use cases.
/// </summary>
internal sealed class ReportManager(IManagerDispatcher dispatcher) : IReportManager
{
    public Task<Result<BookingSummaryModel>> GetBookingSummaryAsync(CancellationToken cancellationToken) =>
        dispatcher.Send(new GetBookingSummaryRequest(), cancellationToken);
}
