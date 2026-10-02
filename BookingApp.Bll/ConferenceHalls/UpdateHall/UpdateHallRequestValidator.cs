using FluentValidation;

namespace BookingApp.Bll.ConferenceHalls.UpdateHall;

public class UpdateHallRequestValidator : AbstractValidator<UpdateHallRequest>
{
    public UpdateHallRequestValidator()
    {
        RuleFor(command => command.HallId)
            .NotEmpty();

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