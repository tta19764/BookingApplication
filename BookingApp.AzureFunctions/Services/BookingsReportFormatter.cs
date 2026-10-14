using System.Globalization;
using System.Text;
using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.AzureFunctions.Services;

/// <summary>Formats plain text in bounded chunks without splitting surrogate pairs.</summary>
public sealed class BookingsReportFormatter : IBookingsReportFormatter
{
    /// <inheritdoc />
    public IReadOnlyList<string> Format(DateOnly date, TimeZoneInfo zone, IReadOnlyList<BookingReportRow> rows)
    {
        var text = new StringBuilder($"Bookings for {date:yyyy-MM-dd} ({zone.Id})\nCount: {rows.Count}\n");
        if (rows.Count == 0) text.AppendLine("No bookings today.");
        foreach (var row in rows.OrderBy(r => r.StartUtc).ThenBy(r => r.Id))
        {
            var start = TimeZoneInfo.ConvertTimeFromUtc(row.StartUtc, zone);
            var end = TimeZoneInfo.ConvertTimeFromUtc(row.EndUtc, zone);
            text.AppendLine(FormattableString.Invariant(
                $"\nHall: {row.HallName}\n{start:yyyy-MM-dd HH:mm} – {end:yyyy-MM-dd HH:mm}\nStatus: {row.Status}\nPrice: {row.TotalPrice:0.00} {row.Currency}"));
        }
        // Cancelled/rejected rows are displayed but excluded from booked-value totals.
        foreach (var currency in rows.Where(r => r.Status is "Reserved" or "Completed")
                     .GroupBy(r => r.Currency).OrderBy(g => g.Key))
            text.AppendLine($"Booked total: {currency.Sum(r => r.TotalPrice).ToString("0.00", CultureInfo.InvariantCulture)} {currency.Key}");

        var value = text.ToString();
        var chunks = new List<string>();
        for (var offset = 0; offset < value.Length;)
        {
            var length = Math.Min(4000, value.Length - offset);
            if (offset + length < value.Length && char.IsHighSurrogate(value[offset + length - 1])) length--;
            chunks.Add(value.Substring(offset, length));
            offset += length;
        }
        return chunks;
    }
}

