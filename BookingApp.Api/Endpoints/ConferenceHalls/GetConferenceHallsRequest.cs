namespace BookingApp.Api.Endpoints.ConferenceHalls;

/// <summary>
/// Query-string pagination request for conference hall lists.
/// </summary>
public sealed record GetConferenceHallsRequest(int Page = 1, int PageSize = 20);
