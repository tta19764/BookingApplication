using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Services.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookingApp.Services.Web.IntegrationTests.Architecture;

public sealed class ExceptionLoggingTests
{
    [Theory]
    [InlineData("validation", 400, "ValidationFailure")]
    [InlineData("unauthorized", 401, "Unauthorized")]
    [InlineData("missing", 404, "NotFound")]
    [InlineData("timeout", 408, "RequestTimeout")]
    public async Task ExpectedFailures_LogInformationWithoutStackTrace(string kind, int status, string type)
    {
        Exception error = kind switch
        {
            "validation" => new ValidationException([new ValidationError("EndTime", "End must follow start.")]),
            "unauthorized" => new UnauthorizedAccessException("private detail"),
            "missing" => new KeyNotFoundException("private detail"),
            _ => new TaskCanceledException("private detail")
        };
        var logs = new CapturingLogger();
        var context = await HandleAsync(error, logs);
        context.Response.StatusCode.Should().Be(status);
        logs.Entries.Should().ContainSingle();
        var entry = logs.Entries.Single();
        entry.Level.Should().Be(LogLevel.Information);
        entry.Exception.Should().BeNull();
        entry.Message.Should().Contain(type).And.Contain(status.ToString()).And.NotContain("private detail")
            .And.NotContain("unhandled");
        context.Response.Body.Position = 0;
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        response.Should().Contain(type);
        if (kind == "validation") response.Should().Contain("EndTime").And.Contain("End must follow start.");
    }

    [Fact]
    public async Task UnexpectedFailure_LogsErrorWithExceptionAndReturnsGenericResponse()
    {
        var error = new InvalidOperationException("private detail");
        var logs = new CapturingLogger();
        var context = await HandleAsync(error, logs);
        context.Response.StatusCode.Should().Be(500);
        logs.Entries.Should().ContainSingle();
        logs.Entries.Single().Level.Should().Be(LogLevel.Error);
        logs.Entries.Single().Exception.Should().BeSameAs(error);
        logs.Entries.Single().Message.Should().Contain("ServerError").And.NotContain("unhandled");
        context.Response.Body.Position = 0;
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        response.Should().NotContain("private detail");
    }

    [Fact]
    public async Task PersistenceFailure_DoesNotDuplicateDalLog()
    {
        var logs = new CapturingLogger();
        var context = await HandleAsync(new PersistenceException(PersistenceError.Failure, "Test", Guid.NewGuid()), logs);
        context.Response.StatusCode.Should().Be(500);
        logs.Entries.Should().BeEmpty();
    }

    private static async Task<DefaultHttpContext> HandleAsync(Exception exception, CapturingLogger logger)
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
        return context;
    }

    private sealed class CapturingLogger : ILogger<ExceptionHandlingMiddleware>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Entries.Add((level, exception, formatter(state, exception)));
    }
}
