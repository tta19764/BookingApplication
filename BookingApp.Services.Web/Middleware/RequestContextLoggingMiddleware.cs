using Serilog.Context;

namespace BookingApp.Services.Web.Middleware;

public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            return next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers
            .TryGetValue(CorrelationIdHeader, out var correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}