using Asp.Versioning.ApiExplorer;
using BookingApp.Services.Web.Middleware;

namespace BookingApp.Services.Web.DI;

/// <summary>
/// Configures API middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Applies the custom exception handling middleware.
    /// </summary>
    public static IApplicationBuilder UseCustomExceptionManager(this IApplicationBuilder app)
    {
        app.Use(next =>
        {
            var middleware = new ExceptionHandlingMiddleware(next,
                app.ApplicationServices.GetRequiredService<ILogger<ExceptionHandlingMiddleware>>());
            return context => middleware.InvokeAsync(context, context.RequestAborted);
        });

        return app;
    }

    /// <summary>
    /// Adds correlation id enrichment to request logs.
    /// </summary>
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.Use(next =>
        {
            var middleware = new RequestContextLoggingMiddleware(next);
            return context => middleware.Invoke(context, context.RequestAborted);
        });

        return app;
    }

    /// <summary>
    /// Enables Swagger and Swagger UI for every discovered API version.
    /// </summary>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        var descriptions = app.DescribeApiVersions();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in descriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"Booking API {description.GroupName}");
            }

            options.RoutePrefix = "swagger";
        });

        return app;
    }

}
