using FluentValidation;

namespace BookingApp.Application.ConferenceHalls.RemoveHall;

public class RemoveHallCommandValidator : AbstractValidator<RemoveHallCommand>
{
    public RemoveHallCommandValidator()
    {
        RuleFor(command => command.HallId)
            .NotEmpty();
    }
}
