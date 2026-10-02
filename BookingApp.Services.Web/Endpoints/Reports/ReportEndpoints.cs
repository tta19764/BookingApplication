using BookingApp.Services.Web.Contracts;
using BookingApp.Services.Web.Dtos;
using BookingApp.Services.Web.Extensions;
using BookingApp.Services.Web.Mappings;
using AutoMapper;
using BookingApp.Bll.Common.Reports;

namespace BookingApp.Services.Web.Endpoints.Reports;

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
            .Produces<ApiResponse<BookingSummaryDto>>();

        return builder;
    }

    public static async Task<IResult> GetBookingSummary(
        IReportManager reportManager,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await reportManager.GetBookingSummaryAsync(cancellationToken);

        return Results.Ok(result.MapToApiResponse(mapper.Map<BookingSummaryDto>));
    }
}
