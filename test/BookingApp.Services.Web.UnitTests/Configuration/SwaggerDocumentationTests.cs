using BookingApp.Services.Web.Controllers;
using BookingApp.Services.Web.DI;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace BookingApp.Services.Web.UnitTests.Configuration;

public sealed class SwaggerDocumentationTests
{
    [Fact]
    public async Task Swagger_UsesXmlCommentsForActionsAndDtoProperties()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(BookingsController).Assembly.FullName,
            EnvironmentName = "Testing"
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=localhost;Database=BookingDocumentation;Integrated Security=true;TrustServerCertificate=true",
            ["BackgroundJobs:CompleteBookings:Enabled"] = "false",
            ["DatabaseSeeding:Enabled"] = "false"
        });
        builder.Services.AddApi(builder.Configuration);
        await using var app = builder.Build();
        app.MapControllers();

        var document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        document.Paths["/api/v1/bookings"].Operations!.Values.Should()
            .Contain(operation => operation.Summary == "Gets a page of bookings.");
        document.Components!.Schemas!["BookingDto"].Properties!["hallId"].Description.Should()
            .Be("Identifier of the conference hall.");
        document.Components.Schemas["CreateBookingRequest"].Properties!["date"].Description.Should()
            .Be("UTC calendar date of the rental.");
    }
}
