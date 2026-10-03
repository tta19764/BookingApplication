namespace BookingApp.Application.Reports.GetBookingSummary;

/// <summary>
/// Booking analytics summary across all halls.
/// </summary>
public sealed record BookingSummaryResponse(
    int TotalBookings,
    decimal TotalRevenue,
    string Currency,
    IReadOnlyCollection<HallBookingSummaryResponse> Halls);

/// <summary>
/// Booking count and revenue for a single hall.
/// </summary>
public sealed record HallBookingSummaryResponse(
    Guid HallId,
    int BookingCount,
    decimal Revenue);
