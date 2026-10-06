using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.Bll.Managers.Bookings;

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
