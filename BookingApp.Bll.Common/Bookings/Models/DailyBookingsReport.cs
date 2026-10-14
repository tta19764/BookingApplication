namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>Calendar context and booking data for a daily report.</summary>
/// <param name="Date">Local reporting date.</param>
/// <param name="TimeZoneId">Time zone used to calculate the day.</param>
/// <param name="Bookings">Rentals overlapping the reporting day.</param>
public sealed record DailyBookingsReport(DateOnly Date, string TimeZoneId, IReadOnlyList<BookingReportRow> Bookings);
