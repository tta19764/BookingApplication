using FluentValidation;

namespace BookingApp.Bll.ConferenceHalls.RemoveHall;

public class RemoveHallRequestValidator : AbstractValidator<RemoveHallRequest>
{
    public RemoveHallRequestValidator()
    {
        RuleFor(command => command.HallId)
            .NotEmpty();
    }
}
