using BookingApp.Bll.Bookings;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Reports;
using BookingApp.Bll.ConferenceHalls;
using BookingApp.Bll.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll;

/// <summary>
/// Registers business-logic managers and validation.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the application layer to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingManager, BookingManager>();
        services.AddScoped<IConferenceHallManager, ConferenceHallManager>();
        services.AddScoped<IReportManager, ReportManager>();

        services.AddTransient<IPricingManager, PricingManager>();

        return services;
    }

}
