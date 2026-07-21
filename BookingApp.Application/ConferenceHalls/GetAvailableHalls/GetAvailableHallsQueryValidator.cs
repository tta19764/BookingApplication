using FluentValidation;

namespace BookingApp.Application.ConferenceHalls.GetAvailableHalls;

public class GetAvailableHallsQueryValidator : AbstractValidator<GetAvailableHallsQuery>
{
    public GetAvailableHallsQueryValidator()
    {
        RuleFor(query => query.Start)
            .NotEmpty();

        RuleFor(query => query.End)
            .GreaterThan(query => query.Start);

        RuleFor(query => query)
            .Must(query => query.Start.Date == query.End.Date)
            .WithMessage("Booking period must be within one calendar day.")
            .Must(query => query.Start.TimeOfDay >= TimeSpan.FromHours(6) &&
                           query.End.TimeOfDay <= TimeSpan.FromHours(23))
            .WithMessage("Booking period must be between 06:00 and 23:00.");

        RuleFor(query => query.Capacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);
    }
}
