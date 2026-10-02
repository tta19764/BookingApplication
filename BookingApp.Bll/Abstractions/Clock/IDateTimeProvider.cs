namespace BookingApp.Bll.Abstractions.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}