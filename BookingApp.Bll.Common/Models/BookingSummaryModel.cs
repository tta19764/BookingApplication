namespace BookingApp.Bll.Common.Models;

/// <summary>
/// Booking analytics summary across all halls.
/// </summary>
public sealed record BookingSummaryModel(
    int TotalBookings,
    decimal TotalRevenue,
    string Currency,
    IReadOnlyCollection<HallBookingSummaryModel> Halls);

/// <summary>
/// Booking count and revenue for a single hall.
/// </summary>
public sealed record HallBookingSummaryModel(
    Guid HallId,
    int BookingCount,
    decimal Revenue);
