using BookingApp.Bll.Common.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Services.Web.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>Runs the request pipeline and converts failures to problem responses.</summary>
    /// <param name="context">The current HTTP request and response.</param>
    /// <param name="cancellationToken">The request abort token supplied by middleware registration.</param>
    /// <returns>A task completed after the request or problem response finishes.</returns>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await next(context);

            await HandleStatusResponseAsync(context, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // A disconnected client cannot receive a problem response.
            throw;
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception, cancellationToken);
        }
    }

    private async Task HandleStatusResponseAsync(HttpContext context, CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted) return;

        var problemDetails = context.Response.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "Unauthorized",
                Title = "Unauthorized",
                Detail = "Authentication is required to access this resource."
            },
            StatusCodes.Status403Forbidden => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "Forbidden",
                Title = "Forbidden",
                Detail = "You do not have permission to access this resource."
            },
            StatusCodes.Status429TooManyRequests => new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Type = "TooManyRequests",
                Title = "Too Many Requests",
                Detail = "Too many requests have been made. Please try again later."
            },
            _ => null
        };

        if (problemDetails is not null)
        {
            await ModifyHeaderAsync(context, problemDetails, cancellationToken);
        }
    }

    private static async Task ModifyHeaderAsync(HttpContext context, ProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionDetails = GetExceptionDetails(exception);
        if (exceptionDetails.LoggingLevel is { } level)
        {
            // Expected request failures need no stack trace; DAL owns persistence diagnostics.
            logger.Log(level, level == LogLevel.Error ? exception : null,
                "Request failed with {FailureType} and HTTP {StatusCode}",
                exceptionDetails.Type, exceptionDetails.Status);
        }

        var problemDetails = new ProblemDetails
        {
            Status = exceptionDetails.Status,
            Type = exceptionDetails.Type,
            Title = exceptionDetails.Title,
            Detail = exceptionDetails.Detail,
        };

        if (exceptionDetails.Errors is not null)
        {
            problemDetails.Extensions["errors"] = exceptionDetails.Errors;
        }

        if (exception is PersistenceException persistence)
            problemDetails.Extensions["incidentId"] = persistence.IncidentId;

        context.Response.StatusCode = exceptionDetails.Status;
        await ModifyHeaderAsync(context, problemDetails, cancellationToken);
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            PersistenceException => new ExceptionDetails(
                StatusCodes.Status500InternalServerError,
                "PersistenceFailure",
                "Database operation failed",
                "The database operation could not be completed.",
                null, LoggingLevel: null),

            ValidationException validationException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "ValidationFailure",
                "Validation Error",
                "One or more validation errors have occurred.",
                validationException.Errors),

            UnauthorizedAccessException => new ExceptionDetails(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Unauthorized",
                "You are not authorized to perform this action.",
                null),

            KeyNotFoundException => new ExceptionDetails(
                StatusCodes.Status404NotFound,
                "NotFound",
                "Resource Not Found",
                "The requested resource was not found.",
                null),

            TaskCanceledException => new ExceptionDetails(
                StatusCodes.Status408RequestTimeout,
                "RequestTimeout",
                "Request Timeout",
                "The request timed out.",
                null),

            _ => new ExceptionDetails(
                StatusCodes.Status500InternalServerError,
                "ServerError",
                "Server Error",
                "An unexpected error has occurred on the server.",
                null, LoggingLevel: LogLevel.Error)
        };
    }

    private record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors,
        LogLevel? LoggingLevel = LogLevel.Information);
}
