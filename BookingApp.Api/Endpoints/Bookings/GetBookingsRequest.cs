namespace BookingApp.Api.Endpoints.Bookings;

/// <summary>
/// Query-string pagination request for booking lists.
/// </summary>
public sealed record GetBookingsRequest(int Page = 1, int PageSize = 20);
