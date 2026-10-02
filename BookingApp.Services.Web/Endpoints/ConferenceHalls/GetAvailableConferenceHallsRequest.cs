namespace BookingApp.Services.Web.Endpoints.ConferenceHalls;

/// <summary>
/// Request parameters for finding available conference halls.
/// </summary>
public sealed record GetAvailableConferenceHallsRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity);
