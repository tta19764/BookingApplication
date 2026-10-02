namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request parameters for finding available conference halls.
/// </summary>
public sealed record GetAvailableConferenceHallsRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity);
