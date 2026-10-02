namespace BookingApp.Bll.Common.Bookings;

public sealed record DateRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public TimeSpan Duration => End - Start;

    public static DateRange Create(DateTime start, DateTime end) => new() { Start = start, End = end };

    public static DateRange Create(DateOnly date, TimeOnly start, TimeOnly end) => new()
    {
        Start = DateTime.SpecifyKind(date.ToDateTime(start), DateTimeKind.Utc),
        End = DateTime.SpecifyKind(date.ToDateTime(end), DateTimeKind.Utc)
    };
}
