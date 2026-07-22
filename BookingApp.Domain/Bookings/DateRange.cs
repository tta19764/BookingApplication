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
    /// Creates a booking period from one date and minute-level start/end times.
    /// </summary>
    public static DateRange Create(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (date == default)
        {
            throw new ArgumentException("Booking date is required.", nameof(date));
        }

        if (startTime < new TimeOnly(6, 0) || startTime >= new TimeOnly(23, 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(startTime),
                "Start time must be between 06:00 and 22:59.");
        }

        if (endTime <= new TimeOnly(6, 0) || endTime > new TimeOnly(23, 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(endTime),
                "End time must be between 06:01 and 23:00.");
        }

        if (startTime >= endTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }

        return Create(
            date.ToDateTime(startTime),
            date.ToDateTime(endTime));
    }

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
