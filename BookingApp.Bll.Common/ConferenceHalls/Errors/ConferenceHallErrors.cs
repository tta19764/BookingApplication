using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.ConferenceHalls.Errors;

public static class ConferenceHallErrors
{
    public static readonly Error HasBookings = new(
        "ConferenceHall.HasBookings", "A hall with bookings cannot be removed");
    public static readonly Error NotFound = new(
        "ConferenceHall.NotFound",
        "The conference hall with the specified identifier was not found");
}
