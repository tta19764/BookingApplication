using BookingApp.Api.Endpoints;
using BookingApp.Api.Endpoints.Bookings;
using BookingApp.Api.Endpoints.ConferenceHalls;
using BookingApp.Api.Endpoints.Reports;

namespace BookingApp.Api.Extensions;

/// <summary>
/// Central place for mapping all minimal API endpoint groups.
/// </summary>
public static class EndpointMappings
{
    /// <summary>
    /// Maps all versioned application endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(BookingAppApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapConferenceHallEndpoints();
        api.MapBookingEndpoints();
        api.MapReportEndpoints();

        return builder;
    }
}
