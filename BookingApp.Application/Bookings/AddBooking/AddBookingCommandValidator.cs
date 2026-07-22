using FluentValidation;
using System.Globalization;

namespace BookingApp.Application.Bookings.AddBooking;

public class AddBookingCommandValidator : AbstractValidator<AddBookingCommand>
{
    public AddBookingCommandValidator()
    {
        RuleFor(command => command.HallId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Date)
            .NotEmpty();

        RuleFor(command => command.StartTime)
            .NotEmpty()
            .Must(BeValidStartTime)
            .WithMessage("Start time must use HH:mm format and be between 06:00 and 22:59.");

        RuleFor(command => command.EndTime)
            .NotEmpty()
            .Must(BeValidEndTime)
            .WithMessage("End time must use HH:mm format and be between 06:01 and 23:00.");

        RuleFor(command => command)
            .Must(command => TryParseTime(command.StartTime, out var startTime) &&
                             TryParseTime(command.EndTime, out var endTime) &&
                             startTime < endTime)
            .WithMessage("End time must be after start time.");

        RuleForEach(command => command.Amenities)
            .IsInEnum();
    }

    private static bool BeValidStartTime(string value)
    {
        return TryParseTime(value, out var time) &&
               time >= new TimeOnly(6, 0) &&
               time < new TimeOnly(23, 0);
    }

    private static bool BeValidEndTime(string value)
    {
        return TryParseTime(value, out var time) &&
               time > new TimeOnly(6, 0) &&
               time <= new TimeOnly(23, 0);
    }

    private static bool TryParseTime(string value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(
            value,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }
}