using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Bll.Common.Shared;
using FluentAssertions;

namespace BookingApp.Bll.IntegrationTests.ConferenceHalls;

[Collection("SqlServer")]
public class GetAvailableHallsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetAvailableHalls_Should_ReturnSeededHalls_WhenNoBookingsOverlap()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        const int capacity = 30;

        // Act
        var model = new FindAvailableHallsModel(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            new TimeOnly(10, 40), new TimeOnly(12, 10), capacity);
        Result<IEnumerable<HallModel>> result = await HallManager.GetAvailableHallsAsync(model, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().OnlyContain(hall =>
            hall.Id != Guid.Empty &&
            !string.IsNullOrWhiteSpace(hall.Name) &&
            hall.Capacity >= capacity);
    }
}
