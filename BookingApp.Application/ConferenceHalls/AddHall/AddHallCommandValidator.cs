using FluentValidation;
using BookingApp.Domain.Shared;

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

        RuleFor(command => command.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Must(BeSupportedCurrency)
            .WithMessage("Currency code is not supported.");

        RuleForEach(command => command.Amenities)
            .IsInEnum();
    }

    private static bool BeSupportedCurrency(string currencyCode)
    {
        return Currency.All.Any(currency =>
            string.Equals(currency.Code, currencyCode?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
