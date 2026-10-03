namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request-string pagination request for conference hall lists.
/// </summary>
public sealed record GetConferenceHallsRequest(int Page = 1, int PageSize = 20);
