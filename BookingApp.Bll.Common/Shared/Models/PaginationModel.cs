namespace BookingApp.Bll.Common.Shared.Models;

/// <summary>One-based pagination parameters.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Maximum number of items per page.</param>
public sealed record PaginationModel(int Page, int PageSize)
{
    /// <summary>One-based page number.</summary>
    public int Page { get; init; } = Page;

    /// <summary>Maximum number of items per page.</summary>
    public int PageSize { get; init; } = PageSize;
}
