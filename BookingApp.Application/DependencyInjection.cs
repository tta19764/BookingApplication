using BookingApp.Application.Abstractions.Behaviours;
using BookingApp.Domain.Bookings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Application;

/// <summary>
/// Registers application-layer services, handlers, validators, and MediatR pipeline behaviors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the application layer to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        services.AddTransient<PricingService>();

        return services;
    }
}