using BookingApp.Application.ConferenceHalls.AddHall;
using BookingApp.Application.IntegrationTests.Infrastructure;
using BookingApp.Domain.ConferenceHalls;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Application.IntegrationTests.ConferenceHalls;

public class AddHallTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task AddHall_Should_PersistConferenceHall()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new AddHallCommand(
            $"Integration Hall {Guid.NewGuid():N}",
            42,
            1800m,
            "UAH",
            [Amenity.Projector, Amenity.WiFi]);

        // Act
        var result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var hall = await DbContext
            .Set<ConferenceHall>()
            .AsNoTracking()
            .FirstOrDefaultAsync(conferenceHall => conferenceHall.Id == result.Value, cancellationToken);

        hall.Should().NotBeNull();
        hall.Name.Value.Should().Be(command.Name);
        hall.Seats.Value.Should().Be(command.Capacity);
        hall.Price.Currency.Code.Should().Be(command.CurrencyCode);
    }
}
