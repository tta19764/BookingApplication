using BookingApp.Services.Web.Endpoints;
using BookingApp.Services.Web.Endpoints.Bookings;
using BookingApp.Services.Web.Endpoints.ConferenceHalls;
using BookingApp.Services.Web.Endpoints.Reports;

namespace BookingApp.Services.Web.Extensions;

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
