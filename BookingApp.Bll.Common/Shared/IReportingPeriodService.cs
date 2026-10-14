namespace BookingApp.Bll.Common.Shared;

/// <summary>Calculates calendar-day reporting boundaries.</summary>
public interface IReportingPeriodService
{
    /// <summary>Calculates consecutive local midnights converted to UTC.</summary>
    /// <param name="now">Instant whose local calendar day is selected.</param>
    /// <param name="zone">Time zone used to determine local midnights.</param>
    /// <returns>The local date and its half-open UTC interval.</returns>
    ReportingPeriod GetPeriod(DateTimeOffset now, TimeZoneInfo zone);
}
