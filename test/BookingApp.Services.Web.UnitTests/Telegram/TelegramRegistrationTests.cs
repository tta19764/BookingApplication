using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Services.Web.DI;
using BookingApp.Services.Web.Services.Telegram;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookingApp.Services.Web.UnitTests.Telegram;

public sealed class TelegramRegistrationTests
{
    [Theory]
    [InlineData(false, "", "", true)]
    [InlineData(true, "123:token", "-100123", true)]
    [InlineData(true, "", "-100123", false)]
    [InlineData(true, "123:token", "", false)]
    public void Registration_ValidatesConfigurationAndResolvesHandler(bool enabled, string token, string chat, bool valid)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=localhost;Database=Booking;Integrated Security=true",
            ["BackgroundJobs:CompleteBookings:Enabled"] = "false",
            ["Telegram:Enabled"] = enabled.ToString(),
            ["Telegram:BotToken"] = token,
            ["Telegram:ChatId"] = chat
        }).Build();
        using var provider = new ServiceCollection().AddLogging().AddApi(configuration).BuildServiceProvider();
        using var scope = provider.CreateScope();
        Func<TelegramOptions> resolve = () => provider.GetRequiredService<IOptions<TelegramOptions>>().Value;
        if (!valid)
        {
            resolve.Should().Throw<OptionsValidationException>();
            return;
        }

        resolve().Enabled.Should().Be(enabled);
        scope.ServiceProvider.GetServices<IDomainEventHandler<BookingCreatedDomainEvent>>()
            .Should().Contain(handler => handler is TelegramBookingCreatedEventHandler);
    }
}
