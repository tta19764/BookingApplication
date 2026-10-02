namespace BookingApp.Services.Web.Endpoints.ConferenceHalls;

/// <summary>
/// Request parameters for finding available conference halls.
/// </summary>
public sealed record GetAvailableConferenceHallsRequest(
    DateOnly Date,
    string StartTime,
    string EndTime,
    int Capacity);
