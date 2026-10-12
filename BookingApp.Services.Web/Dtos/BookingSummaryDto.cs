namespace BookingApp.Services.Web.Dtos;

/// <summary>Booking counts and revenue across all halls.</summary>
/// <param name="TotalBookings">Total number of bookings.</param>
/// <param name="TotalRevenue">Total booking revenue.</param>
/// <param name="Currency">Currency code for the monetary amounts.</param>
/// <param name="Halls">Booking counts and revenue grouped by hall.</param>
public sealed record BookingSummaryDto(
    int TotalBookings,
    decimal TotalRevenue,
    string Currency,
    IReadOnlyCollection<HallBookingSummaryDto> Halls)
{
    /// <summary>Total number of bookings.</summary>
    public int TotalBookings { get; init; } = TotalBookings;

    /// <summary>Total booking revenue.</summary>
    public decimal TotalRevenue { get; init; } = TotalRevenue;

    /// <summary>Currency code for the monetary amounts.</summary>
    public string Currency { get; init; } = Currency;

    /// <summary>Booking counts and revenue grouped by hall.</summary>
    public IReadOnlyCollection<HallBookingSummaryDto> Halls { get; init; } = Halls;
}

/// <summary>Booking count and revenue for a single hall.</summary>
/// <param name="HallId">Identifier of the conference hall.</param>
/// <param name="BookingCount">Number of bookings for the hall.</param>
/// <param name="Revenue">Booking revenue for the hall.</param>
public sealed record HallBookingSummaryDto(Guid HallId, int BookingCount, decimal Revenue)
{
    /// <summary>Identifier of the conference hall.</summary>
    public Guid HallId { get; init; } = HallId;

    /// <summary>Number of bookings for the hall.</summary>
    public int BookingCount { get; init; } = BookingCount;

    /// <summary>Booking revenue for the hall.</summary>
    public decimal Revenue { get; init; } = Revenue;
}
