using BookingApp.AzureFunctions.Configuration;
using BookingApp.AzureFunctions.Functions;
using BookingApp.AzureFunctions.Services;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BookingApp.AzureFunctions.UnitTests;

public sealed class FunctionsConfigurationTests
{
    [Fact]
    public void EnabledReportDependenciesValidateAndResolveWithoutAutoMapper()
    {
        var builder = FunctionsApplication.CreateBuilder([]);
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["KeyVault:Uri"] = "",
            ["BookingsReport:Enabled"] = "true",
            ["ConnectionStrings:Database"] = "Server=localhost;Database=ReportTests;Integrated Security=true",
            ["Telegram:BotToken"] = "test-token",
            ["Telegram:ChatId"] = "test-chat"
        });
        builder.AddBookingsReporting();
        // Validate the actual registrations without starting the worker or contacting external systems.
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IBookingReportService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IBookingsReportFormatter>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ITelegramReportSender>());
        Assert.NotNull(ActivatorUtilities.CreateInstance<BookingsReportSchedule>(scope.ServiceProvider));
        Assert.Null(scope.ServiceProvider.GetService<IConferenceHallRepository>());
    }
}
