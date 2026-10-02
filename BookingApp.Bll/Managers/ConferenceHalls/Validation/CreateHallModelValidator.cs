using BookingApp.Bll.Common.ConferenceHalls.Models;
using FluentValidation;

namespace BookingApp.Bll.Managers.ConferenceHalls.Validation;

public sealed class CreateHallModelValidator : AbstractValidator<CreateHallModel>
{
    public CreateHallModelValidator()
    {
        RuleFor(model => model.Name).NotEmpty().MaximumLength(100);
        RuleFor(model => model.Capacity).InclusiveBetween(1, 1000);
        RuleFor(model => model.HourlyRate).GreaterThan(0);
        RuleFor(model => model.CurrencyCode)
            .Must(code => code is not null && Currency.All.Any(currency =>
                string.Equals(currency.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Currency code is not supported.");
        RuleFor(model => model.Amenities).NotNull();
        RuleForEach(model => model.Amenities).IsInEnum();
    }
}
