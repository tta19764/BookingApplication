using BookingApp.Application.Abstractions.Events;
using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Bookings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Application;

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
        services.AddScoped<IApplicationDispatcher, ApplicationDispatcher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        RegisterHandlers(services, applicationAssembly.DefinedTypes);
        
        services.AddTransient<PricingService>();

        return services;
    }

    private static void RegisterHandlers(
        IServiceCollection services,
        IEnumerable<System.Reflection.TypeInfo> applicationTypes)
    {
        var handlerDefinitions = new[]
        {
            typeof(IApplicationRequestHandler<,>),
            typeof(IDomainEventHandler<>)
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
