using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Services.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookingApp.Services.Web.UnitTests.Middleware;

public sealed class PersistenceErrorTests
{
    [Fact]
    public async Task Middleware_ReturnsSanitizedFailureAndIncidentId()
    {
        var error = new PersistenceException(PersistenceError.AccessDenied, "UserRepository.AddAsync", Guid.NewGuid());
        var middleware = new ExceptionHandlingMiddleware(_ => throw error,
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
        context.Response.StatusCode.Should().Be(500);
        context.Response.Body.Position = 0;
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        response.Should().Contain(error.IncidentId.ToString()).And.Contain("PersistenceFailure")
            .And.NotContain("UserRepository").And.NotContain("AccessDenied");
        error.InnerException.Should().BeNull();
    }
}
