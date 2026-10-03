using BookingApp.Domain.Abstractions;

namespace BookingApp.Domain.Bookings;

public static class BookingErrors
{
    public static Error NotFound = new(
        "Booking.Found",
        "The booking with the specified identifier was not found");

    public static Error Overlap = new(
        "Booking.Overlap",
        "The current booking is overlapping with an existing one");

    public static Error NotReserved = new(
        "Booking.NotReserved",
        "The booking is not pending");

    public static Error AlreadyStarted = new(
        "Booking.AlreadyStarted",
        "The booking has already started");

    public static Error StartsInPast = new(
        "Booking.StartsInPast",
        "The booking start time must be in the future");
}
