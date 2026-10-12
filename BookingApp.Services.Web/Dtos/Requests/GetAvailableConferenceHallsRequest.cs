namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request parameters for finding available conference halls.
/// </summary>
/// <param name="Date">UTC calendar date of the rental.</param>
/// <param name="StartTime">Rental start time on the specified UTC date.</param>
/// <param name="EndTime">Rental end time on the specified UTC date.</param>
/// <param name="Capacity">Number of seats required or provided.</param>
public sealed record GetAvailableConferenceHallsRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity)
{
    /// <summary>UTC calendar date of the rental.</summary>
    public DateOnly Date { get; init; } = Date;

    /// <summary>Rental start time on the specified UTC date.</summary>
    public TimeOnly StartTime { get; init; } = StartTime;

    /// <summary>Rental end time on the specified UTC date.</summary>
    public TimeOnly EndTime { get; init; } = EndTime;

    /// <summary>Number of seats required or provided.</summary>
    public int Capacity { get; init; } = Capacity;
}
