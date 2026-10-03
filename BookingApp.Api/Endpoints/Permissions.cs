namespace BookingApp.Api.Endpoints;

/// <summary>
/// Permission names used by API endpoints when authorization is introduced.
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Allows reading conference halls.
    /// </summary>
    public const string ConferenceHallRead = "conference-halls:read";

    /// <summary>
    /// Allows changing conference halls.
    /// </summary>
    public const string ConferenceHallWrite = "conference-halls:write";

    /// <summary>
    /// Allows reading bookings.
    /// </summary>
    public const string BookingRead = "bookings:read";

    /// <summary>
    /// Allows changing bookings.
    /// </summary>
    public const string BookingWrite = "bookings:write";
}
