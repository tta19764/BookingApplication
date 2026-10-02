using BookingApp.Bll.Common.Bookings;

namespace BookingApp.Bll.Bookings;

public static class BookingPeriodFactory
{
    public static DateRange Create(DateOnly date, TimeOnly start, TimeOnly end)
    {
        return new DateRange
        {
            Start = DateTime.SpecifyKind(date.ToDateTime(start), DateTimeKind.Utc),
            End = DateTime.SpecifyKind(date.ToDateTime(end), DateTimeKind.Utc)
        };
    }
}
