using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.Bookings.AddBooking;

public record AddBookingCommand() : ICommand<Guid>;