namespace BookingApp.Bll.Common.Shared;

/// <summary>A local calendar date and its half-open UTC interval.</summary>
/// <param name="Date">Local calendar date.</param>
/// <param name="StartUtc">Inclusive UTC day boundary.</param>
/// <param name="EndUtc">Exclusive UTC day boundary.</param>
public sealed record ReportingPeriod(DateOnly Date, DateTime StartUtc, DateTime EndUtc);
