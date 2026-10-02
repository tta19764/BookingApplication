using BookingApp.Bll.ConferenceHalls.GetAvailableHalls;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Bll.Common.Abstractions;
using FluentAssertions;

namespace BookingApp.Bll.IntegrationTests.ConferenceHalls;

public class GetAvailableHallsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetAvailableHalls_Should_ReturnSeededHalls_WhenNoBookingsOverlap()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var query = new GetAvailableHallsRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            "10:40",
            "12:10",
            30);

        // Act
        Result<IEnumerable<HallResponse>> result = await Sender.Send(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().OnlyContain(hall =>
            hall.Id != Guid.Empty &&
            !string.IsNullOrWhiteSpace(hall.Name) &&
            hall.Capacity >= query.Capacity);
    }
}
