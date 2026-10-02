using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Reports;
using FluentAssertions;
using NSubstitute;

namespace BookingApp.Bll.UnitTests.Bookings;

public class ReportManagerTests
{
    [Fact]
    public async Task GetBookingSummaryAsync_ReturnsEmptySummary_WhenThereAreNoBookings()
    {
        // Arrange
        var repository = Substitute.For<IBookingRepository>();
        repository.List(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(EmptyPages());
        var manager = new ReportManager(repository);

        // Act
        BookingSummaryModel summary = (await manager.GetBookingSummaryAsync(
            TestContext.Current.CancellationToken)).Value;

        // Assert
        summary.TotalBookings.Should().Be(0);
        summary.TotalRevenue.Should().Be(0m);
    }

    private static async IAsyncEnumerable<IReadOnlyCollection<Booking>> EmptyPages()
    {
        await Task.CompletedTask;
        yield break;
    }
}
