using BookingApp.Bll.Common.ConferenceHalls.Models;
using FluentValidation;

namespace BookingApp.Bll.Managers.ConferenceHalls.Validation;

public sealed class HallReferenceModelValidator : AbstractValidator<HallReferenceModel>
{
    public HallReferenceModelValidator()
    {
        RuleFor(model => model.HallId).NotEmpty();
    }
}
