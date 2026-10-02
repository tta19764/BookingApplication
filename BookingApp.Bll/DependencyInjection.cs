using BookingApp.Bll.Bookings;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Reports;
using BookingApp.Bll.ConferenceHalls;
using BookingApp.Bll.Reports;
using FluentValidation;
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
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddScoped<IManagerDispatcher, ManagerDispatcher>();
        services.AddScoped<IBookingManager, BookingManager>();
        services.AddScoped<IConferenceHallManager, ConferenceHallManager>();
        services.AddScoped<IReportManager, ReportManager>();

        RegisterManagers(services, applicationAssembly.DefinedTypes);

        services.AddTransient<IPricingManager, PricingManager>();

        return services;
    }

    private static void RegisterManagers(
        IServiceCollection services,
        IEnumerable<System.Reflection.TypeInfo> applicationTypes)
    {
        var handlerDefinitions = new[] { typeof(IRequestManager<,>) };

        foreach (var implementationType in applicationTypes.Where(type => type is { IsClass: true, IsAbstract: false }))
        {
            foreach (var serviceType in implementationType.ImplementedInterfaces.Where(
                         type => type.IsGenericType && handlerDefinitions.Contains(type.GetGenericTypeDefinition())))
            {
                services.AddTransient(serviceType, implementationType.AsType());
            }
        }
    }
}
