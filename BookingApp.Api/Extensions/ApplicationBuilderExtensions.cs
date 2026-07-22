using Asp.Versioning.ApiExplorer;
using BookingApp.Api.Middleware;
using BookingApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Api.Extensions;

/// <summary>
/// Configures API middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Applies the custom exception handling middleware.
    /// </summary>
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }

    /// <summary>
    /// Adds correlation id enrichment to request logs.
    /// </summary>
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    /// <summary>
    /// Enables Swagger and Swagger UI for every discovered API version.
    /// </summary>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"Booking API {description.GroupName}");
            }

            options.RoutePrefix = "swagger";
        });

        return app;
    }
    
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}
