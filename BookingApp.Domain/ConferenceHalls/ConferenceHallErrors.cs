using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.ConferenceHalls;

public static class ConferenceHallErrors
{
    public static readonly Error NotFound = new(
        "ConferenceHall.NotFound",
        "The conference hall with the specified identifier was not found");
}
