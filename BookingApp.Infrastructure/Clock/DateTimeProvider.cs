using BookingApp.Application.Abstractions.Clock;

namespace BookingApp.Infrastructure.Clock;

/// <summary>
/// System clock implementation used by application services.
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Current UTC time from the system clock.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
