namespace BookingApp.Services.Web.Dtos.Requests;

/// <summary>
/// Request-string pagination request for conference hall lists.
/// </summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Maximum number of items per page.</param>
public sealed record GetConferenceHallsRequest(int Page = 1, int PageSize = 20)
{
    /// <summary>One-based page number.</summary>
    public int Page { get; init; } = Page;

    /// <summary>Maximum number of items per page.</summary>
    public int PageSize { get; init; } = PageSize;
}
