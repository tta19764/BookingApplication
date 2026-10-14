using BookingApp.AzureFunctions.Services;
using BookingApp.Bll.Common.Bookings.Models;
using Xunit;

namespace BookingApp.AzureFunctions.UnitTests;

public class ReportTests
{
    [Fact]
    public void EmptyReportIsExplicit()
    {
        var messages = new BookingsReportFormatter().Format(new DateOnly(2026, 10, 14), TimeZoneInfo.Utc, []);
        Assert.Contains("No bookings today.", Assert.Single(messages));
    }

    [Fact]
    public void TotalsSeparateCurrenciesAndExcludeCancelled()
    {
        var rows = new[] { Row("Reserved", 10, "USD"), Row("Completed", 20, "USD"),
            Row("Cancelled", 100, "USD"), Row("Rejected", 200, "USD"), Row("Reserved", 5, "EUR") };
        var text = string.Concat(new BookingsReportFormatter().Format(new DateOnly(2026, 10, 14), TimeZoneInfo.Utc, rows));
        Assert.Contains("Booked total: 30.00 USD", text);
        Assert.Contains("Booked total: 5.00 EUR", text);
        Assert.Contains("Status: Cancelled", text);
    }

    [Fact]
    public void LongReportsPreserveContentAndSurrogatePairs()
    {
        var rows = Enumerable.Range(0, 120).Select(_ => Row("Reserved", 1, "USD") with { HallName = new string('a', 99) + "😀" }).ToArray();
        var messages = new BookingsReportFormatter().Format(new DateOnly(2026, 10, 14), TimeZoneInfo.Utc, rows);
        Assert.True(messages.Count > 1);
        Assert.All(messages, text => {
            Assert.InRange(text.Length, 1, 4000);
            Assert.False(char.IsHighSurrogate(text[^1]));
            Assert.False(char.IsLowSurrogate(text[0]));
        });
        Assert.Equal(120, string.Concat(messages).Split("Status: Reserved").Length - 1);
    }

    private static BookingReportRow Row(string status, decimal price, string currency) =>
        new(Guid.NewGuid(), "Hall", new DateTime(2026, 10, 14, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 14, 11, 0, 0, DateTimeKind.Utc), status, price, currency);
}





