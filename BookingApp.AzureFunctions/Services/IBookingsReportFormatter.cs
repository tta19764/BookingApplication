using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.AzureFunctions.Services;

/// <summary>Formats daily booking data as bounded message text.</summary>
public interface IBookingsReportFormatter
{
    /// <summary>Formats the report as plain-text messages below the Telegram size limit.</summary>
    /// <param name="date">Local calendar date represented by the report.</param>
    /// <param name="zone">Time zone used to display rental timestamps.</param>
    /// <param name="rows">Rentals and persisted price snapshots.</param>
    /// <returns>Ordered message parts, including an explicit message for an empty day.</returns>
    IReadOnlyList<string> Format(DateOnly date, TimeZoneInfo zone, IReadOnlyList<BookingReportRow> rows);
}
