namespace BookingApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Configuration values that control automatic booking completion.
/// </summary>
internal sealed class CompleteBookingsOptions
{
    /// <summary>
    /// Configuration section used to bind this options object.
    /// </summary>
    public const string SectionName = "BackgroundJobs:CompleteBookings";

    /// <summary>
    /// Number of seconds between job executions.
    /// </summary>
    public int IntervalSeconds { get; init; }

    /// <summary>
    /// Maximum number of expired reservations completed in one repository batch.
    /// </summary>
    public int PageSize { get; init; }
}
