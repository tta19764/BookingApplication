using FluentValidation;

namespace BookingApp.Application.Bookings.AddBooking;

public class AddBookingCommandValidator : AbstractValidator<AddBookingCommand>
{
    public AddBookingCommandValidator()
    {
        RuleFor(command => command.HallId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Start)
            .NotEmpty();

        RuleFor(command => command.End)
            .GreaterThan(command => command.Start);

        RuleFor(command => command)
            .Must(command => command.Start.Date == command.End.Date)
            .WithMessage("Booking period must be within one calendar day.")
            .Must(command => command.Start.TimeOfDay >= TimeSpan.FromHours(6) &&
                             command.End.TimeOfDay <= TimeSpan.FromHours(23))
            .WithMessage("Booking period must be between 06:00 and 23:00.");

        RuleForEach(command => command.Amenities)
            .IsInEnum();
    }
}