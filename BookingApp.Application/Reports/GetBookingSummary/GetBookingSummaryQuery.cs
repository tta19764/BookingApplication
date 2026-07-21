using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.Reports.GetBookingSummary;

/// <summary>
/// Query for retrieving booking volume and revenue analytics.
/// </summary>
public record GetBookingSummaryQuery() : IQuery<BookingSummaryResponse>;
