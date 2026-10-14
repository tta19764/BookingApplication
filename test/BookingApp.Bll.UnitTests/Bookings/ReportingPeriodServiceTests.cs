namespace BookingApp.Bll.UnitTests.Bookings;

public sealed class ReportingPeriodServiceTests
{
    [Theory]
    [InlineData(2026, 3, 29, 23)]
    [InlineData(2026, 10, 25, 25)]
    [InlineData(2026, 10, 14, 24)]
    public void PeriodUsesLocalMidnightsAcrossDst(int year, int month, int day, int hours)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        var period = new BookingApp.Bll.Services.Shared.ReportingPeriodService().GetPeriod(new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero), zone);
        Assert.Equal(new DateOnly(year, month, day), period.Date);
        Assert.Equal(hours, (period.EndUtc - period.StartUtc).TotalHours);
        Assert.Equal(TimeSpan.Zero, TimeZoneInfo.ConvertTimeFromUtc(period.StartUtc, zone).TimeOfDay);
        Assert.Equal(DateTimeKind.Utc, period.StartUtc.Kind);
    }

    [Fact]
    public void LocalDayCanDifferFromUtcDay()
    {
        var period = new BookingApp.Bll.Services.Shared.ReportingPeriodService().GetPeriod(new DateTimeOffset(2026, 10, 14, 22, 0, 0, TimeSpan.Zero),
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv"));
        Assert.Equal(new DateOnly(2026, 10, 15), period.Date);
    }

}

