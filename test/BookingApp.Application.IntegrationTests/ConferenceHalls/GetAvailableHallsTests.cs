using BookingApp.Application.ConferenceHalls.GetAvailableHalls;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Application.IntegrationTests.Infrastructure;
using BookingApp.Domain.Abstractions;
using FluentAssertions;

namespace BookingApp.Application.IntegrationTests.ConferenceHalls;

public class GetAvailableHallsTests : BaseIntegrationTest
{
    public GetAvailableHallsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAvailableHalls_Should_ReturnSeededHalls_WhenNoBookingsOverlap()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var query = new GetAvailableHallsQuery(
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
