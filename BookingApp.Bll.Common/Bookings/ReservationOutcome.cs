namespace BookingApp.Bll.Common.Bookings;

public enum ReservationOutcome
{
    Created = 0,
    Overlap = 1,
    HallNotFound = 2,
    UserNotFound = 3
}
