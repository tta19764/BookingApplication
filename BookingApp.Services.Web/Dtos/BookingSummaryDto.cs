namespace BookingApp.Services.Web.Dtos;

public sealed record BookingSummaryDto(
    int TotalBookings,
    decimal TotalRevenue,
    string Currency,
    IReadOnlyCollection<HallBookingSummaryDto> Halls);

public sealed record HallBookingSummaryDto(Guid HallId, int BookingCount, decimal Revenue);
