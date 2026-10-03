namespace BookingApp.Api.Endpoints.ConferenceHalls;

/// <summary>
/// Query parameters for finding available conference halls.
/// </summary>
public sealed record GetAvailableConferenceHallsRequest(
    DateOnly Date,
    string StartTime,
    string EndTime,
    int Capacity);
