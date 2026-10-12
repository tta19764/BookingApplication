using Serilog.Context;

namespace BookingApp.Services.Web.Middleware;

public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>Enriches logs with the correlation identifier for the full request lifetime.</summary>
    /// <param name="context">The current HTTP request.</param>
    /// <param name="cancellationToken">The request abort token supplied by middleware registration.</param>
    /// <returns>A task completed after downstream request processing finishes.</returns>
    public async Task Invoke(HttpContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            await next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers
            .TryGetValue(CorrelationIdHeader, out var correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}