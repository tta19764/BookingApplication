using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Managers.Bookings;
using NSubstitute;

namespace BookingApp.Bll.UnitTests.Bookings;

public sealed class BookingReportManagerTests
{
    [Fact]
    public async Task GetTodayUsesClockAndPassesUtcBoundariesAndCancellationToRepository()
    {
        var repository = Substitute.For<IBookingReportRepository>();
        var clock = new FixedClock(new DateTimeOffset(2026, 10, 14, 22, 0, 0, TimeSpan.Zero));
        var start = new DateTime(2026, 10, 14, 21, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);
        using var cancellation = new CancellationTokenSource();
        IReadOnlyList<BookingReportRow> rows = [new(Guid.NewGuid(), "Hall", start, end, "Reserved", 10, "USD")];
        repository.GetAsync(start, end, cancellation.Token).Returns(Task.FromResult(rows));
        IBookingReportManager manager = new BookingReportManager(repository, clock, new BookingApp.Bll.Services.Shared.ReportingPeriodService());

        var report = await manager.GetTodayAsync("Europe/Kyiv", cancellation.Token);

        Assert.Equal(new DateOnly(2026, 10, 15), report.Date);
        Assert.Equal("Europe/Kyiv", report.TimeZoneId);
        Assert.Same(rows, report.Bookings);
        await repository.Received(1).GetAsync(start, end, cancellation.Token);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}



