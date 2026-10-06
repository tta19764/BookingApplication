using BookingApp.Bll.Common.ConferenceHalls.Models;
using FluentValidation;

namespace BookingApp.Bll.Managers.ConferenceHalls.Validation;

public sealed class UpdateHallModelValidator : AbstractValidator<UpdateHallModel>
{
    public UpdateHallModelValidator()
    {
        RuleFor(model => model.HallId).NotEmpty();
        RuleFor(model => model.Name).NotEmpty().MaximumLength(100);
        RuleFor(model => model.Capacity).InclusiveBetween(1, 1000);
        RuleFor(model => model.HourlyRate).GreaterThan(0);
        RuleFor(model => model.Amenities).NotNull();
        RuleForEach(model => model.Amenities).IsInEnum();
    }
}
