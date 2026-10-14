using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Services.Shared;

/// <summary>Calculates day boundaries independently, accounting for daylight-saving changes.</summary>
public sealed class ReportingPeriodService : IReportingPeriodService
{
    /// <inheritdoc />
    public ReportingPeriod GetPeriod(DateTimeOffset now, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);
        var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        var midnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new ReportingPeriod(date, TimeZoneInfo.ConvertTimeToUtc(midnight, zone),
            TimeZoneInfo.ConvertTimeToUtc(midnight.AddDays(1), zone));
    }
}
