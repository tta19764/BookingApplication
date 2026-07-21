using FluentValidation;

namespace BookingApp.Application.ConferenceHalls.AddHall;

public class AddHallCommandValidator : AbstractValidator<AddHallCommand>
{
    public AddHallCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Capacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);

        RuleFor(command => command.HourlyRate)
            .GreaterThan(0);

        RuleForEach(command => command.Amenities)
            .IsInEnum();
    }
}
