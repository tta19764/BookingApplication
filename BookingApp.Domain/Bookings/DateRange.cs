namespace BookingApp.Domain.Bookings;

/// <summary>
/// Value object that represents the start and end of a booking period.
/// </summary>
public record DateRange
{
    private DateRange()
    {
    }

    public DateTime Start { get; init; }

    public DateTime End { get; init; }
    
    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Creates a valid date range where the end is after the start.
    /// </summary>
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

    /// <summary>
    /// Returns true when this period intersects another period.
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    /// Returns true when the provided date and time is inside this period.
    /// </summary>
    public bool Contains(DateTime dateTime)
    {
        return dateTime >= Start && dateTime <= End;
    }
}