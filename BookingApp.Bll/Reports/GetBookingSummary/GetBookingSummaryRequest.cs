using BookingApp.Bll.Abstractions.Messaging;

namespace BookingApp.Bll.Reports.GetBookingSummary;

/// <summary>
/// Request for retrieving booking volume and revenue analytics.
/// </summary>
public record GetBookingSummaryRequest() : IManagerRequest<Result<BookingSummaryResponse>>;
