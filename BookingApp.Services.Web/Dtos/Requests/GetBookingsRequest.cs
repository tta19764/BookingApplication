namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request-string pagination request for booking lists.
/// </summary>
public sealed record GetBookingsRequest(int Page = 1, int PageSize = 20);
