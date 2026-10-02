using BookingApp.Bll.Abstractions.Events;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Bookings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Bll;

/// <summary>
/// Registers business-logic handlers, validation, and domain-event dispatching.
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
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        RegisterManagers(services, applicationAssembly.DefinedTypes);
        
        services.AddTransient<PricingService>();

        return services;
    }

    private static void RegisterManagers(
        IServiceCollection services,
        IEnumerable<System.Reflection.TypeInfo> applicationTypes)
    {
        var handlerDefinitions = new[]
        {
            typeof(IRequestManager<,>),
            typeof(IDomainEventManager<>)
        };

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
