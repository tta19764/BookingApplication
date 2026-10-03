using BookingApp.Bll.Common.ConferenceHalls.Models;
using FluentValidation;

namespace BookingApp.Bll.Managers.ConferenceHalls.Validation;

public sealed class FindAvailableHallsModelValidator : AbstractValidator<FindAvailableHallsModel>
{
    public FindAvailableHallsModelValidator()
    {
        RuleFor(model => model.Date).NotEmpty();
        RuleFor(model => model.StartTime)
            .InclusiveBetween(new TimeOnly(6, 0), new TimeOnly(22, 59));
        RuleFor(model => model.EndTime)
            .InclusiveBetween(new TimeOnly(6, 1), new TimeOnly(23, 0))
            .GreaterThan(model => model.StartTime);
        RuleFor(model => model.Capacity).InclusiveBetween(1, 1000);
    }
}
