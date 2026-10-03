using BookingApp.Api.Contracts;
using BookingApp.Api.Extensions;
using BookingApp.Application.Reports.GetBookingSummary;
using MediatR;

namespace BookingApp.Api.Endpoints.Reports;

/// <summary>
/// Minimal API endpoints for business reports.
/// </summary>
public static class ReportEndpoints
{
    /// <summary>
    /// Maps report endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("reports")
            .WithTags("Reports")
            .HasApiVersion(BookingAppApiVersions.V1);

        group.MapGet("bookings-summary", GetBookingSummary)
            .WithName(nameof(GetBookingSummary))
            .WithSummary("Get booking revenue summary")
            .Produces<ApiResponse<BookingSummaryResponse>>();

        return builder;
    }

    public static async Task<IResult> GetBookingSummary(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingSummaryQuery(), cancellationToken);

        return Results.Ok(result.MapToApiResponse());
    }
}
