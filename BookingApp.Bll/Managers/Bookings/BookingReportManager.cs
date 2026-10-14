using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Managers.Bookings;

/// <summary>Retrieves daily rental data using local calendar boundaries converted to UTC.</summary>
/// <param name="repository">Reads rental report projections.</param>
/// <param name="clock">Provides the current instant.</param>
/// <param name="periods">Calculates local calendar boundaries in UTC.</param>
public sealed class BookingReportManager(IBookingReportRepository repository, TimeProvider clock,
    IReportingPeriodService periods) : IBookingReportManager
{
    /// <inheritdoc />
    public async Task<DailyBookingsReport> GetTodayAsync(string timeZoneId, CancellationToken cancellationToken = default)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var period = periods.GetPeriod(clock.GetUtcNow(), zone);
        var bookings = await repository.GetAsync(period.StartUtc, period.EndUtc, cancellationToken);
        return new DailyBookingsReport(period.Date, zone.Id, bookings);
    }
}


