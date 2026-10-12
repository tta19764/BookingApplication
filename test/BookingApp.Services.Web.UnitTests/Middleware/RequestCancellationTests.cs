using BookingApp.Services.Web.DI;
using BookingApp.Services.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace BookingApp.Services.Web.UnitTests.Middleware;

public sealed class RequestCancellationTests
{
    [Fact]
    public async Task AbortedRequest_PropagatesCancellationWithoutWritingAnErrorResponse()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(_ =>
        {
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        }, NullLogger<ExceptionHandlingMiddleware>.Instance);

        // Act
        Func<Task> action = () => middleware.InvokeAsync(context, context.RequestAborted);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        context.Response.Body.Length.Should().Be(0);
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task ProblemResponse_CanBeCancelledThroughTheRequestAbortToken()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        var stream = new TokenCapturingStream(cancellation);
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        context.Response.Body = stream;
        await using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        app.UseCustomExceptionManager();
        app.Run(httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        });

        // Act
        Func<Task> action = () => app.Build()(context);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        stream.Tokens.Should().NotBeEmpty().And.OnlyContain(token => token.IsCancellationRequested);
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task CorrelationScope_RemainsActiveAfterDownstreamAwaitAndEndsWithTheRequest()
    {
        // Arrange
        var sink = new CapturingSink();
        using var logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(sink).CreateLogger();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "request-123";
        var middleware = new RequestContextLoggingMiddleware(async _ =>
        {
            await Task.Yield();
            logger.Information("Inside request");
        });

        // Act
        await middleware.Invoke(context, TestContext.Current.CancellationToken);
        logger.Information("After request");

        // Assert
        sink.Events[0].Properties["CorrelationId"].Should().Be(new ScalarValue("request-123"));
        sink.Events[1].Properties.Should().NotContainKey("CorrelationId");
    }

    private sealed class CapturingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    private sealed class TokenCapturingStream(CancellationTokenSource requestCancellation) : MemoryStream
    {
        public List<CancellationToken> Tokens { get; } = [];

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Tokens.Add(cancellationToken);
            requestCancellation.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return base.WriteAsync(buffer, cancellationToken);
        }
    }
}
