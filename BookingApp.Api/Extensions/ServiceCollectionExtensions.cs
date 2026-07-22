using Asp.Versioning;
using BookingApp.Api.Endpoints;
using BookingApp.Api.OpenApi;
using System.Text.Json.Serialization;

namespace BookingApp.Api.Extensions;

/// <summary>
/// Registers API-layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI, JSON settings, and problem details.
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        return services;
    }
}
