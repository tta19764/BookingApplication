namespace BookingApp.Domain.Bookings;

public record DateRange
{
    private DateRange()
    {
    }

    public DateTime Start { get; init; }

    public DateTime End { get; init; }
    
    public TimeSpan Duration => End - Start;

    public static DateRange Create(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("End time must be after start time.");
        }
            
        return new DateRange
        {
            Start = start,
            End = end
        };
    }

    public bool Overlaps(DateRange other)
    {
        return Start < other.End && End > other.Start;
    }

    public bool Contains(DateTime dateTime)
    {
        return dateTime >= Start && dateTime <= End;
    }
}