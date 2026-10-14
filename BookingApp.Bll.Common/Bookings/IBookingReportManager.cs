using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.Bll.Common.Bookings;

/// <summary>Builds reporting data independently of message formatting and delivery.</summary>
public interface IBookingReportManager
{
    /// <summary>Returns rentals overlapping today's calendar day in the specified time zone.</summary>
    /// <param name="timeZoneId">Identifier of the calendar time zone.</param>
    /// <param name="cancellationToken">Cancels the report query.</param>
    /// <returns>Calendar context and rentals overlapping the current day.</returns>
    Task<DailyBookingsReport> GetTodayAsync(string timeZoneId, CancellationToken cancellationToken = default);
}


