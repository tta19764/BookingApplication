using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using FluentAssertions;

namespace BookingApp.Bll.IntegrationTests.ConferenceHalls;

[Collection("SqlServer")]
public class AddHallTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task AddHall_Should_PersistConferenceHall()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var name = $"Integration Hall {Guid.NewGuid():N}";

        // Act
        var model = new CreateHallModel(name, 42, 1800m, "UAH", [Amenity.Projector, Amenity.WiFi]);
        var result = await HallManager.AddHallAsync(model, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var hall = await Halls.GetByIdAsync(result.Value, cancellationToken);

        hall.Should().NotBeNull();
        hall.Name.Value.Should().Be(name);
        hall.Seats.Value.Should().Be(42);
        hall.Price.Currency.Code.Should().Be("UAH");
    }
}
