using FluentValidation;
using System.Globalization;

namespace BookingApp.Application.ConferenceHalls.GetAvailableHalls;

public class GetAvailableHallsQueryValidator : AbstractValidator<GetAvailableHallsQuery>
{
    public GetAvailableHallsQueryValidator()
    {
        RuleFor(query => query.Date)
            .NotEmpty();

        RuleFor(query => query.StartTime)
            .NotEmpty()
            .Must(BeValidStartTime)
            .WithMessage("Start time must use HH:mm format and be between 06:00 and 22:59.");

        RuleFor(query => query.EndTime)
            .NotEmpty()
            .Must(BeValidEndTime)
            .WithMessage("End time must use HH:mm format and be between 06:01 and 23:00.");

        RuleFor(query => query)
            .Must(query => TryParseTime(query.StartTime, out var startTime) &&
                           TryParseTime(query.EndTime, out var endTime) &&
                           startTime < endTime)
            .WithMessage("End time must be after start time.");

        RuleFor(query => query.Capacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);
    }

    private static bool BeValidStartTime(string value)
    {
        return TryParseTime(value, out var time) &&
               time >= new TimeOnly(6, 0) &&
               time < new TimeOnly(23, 0);
    }

    private static bool BeValidEndTime(string value)
    {
        return TryParseTime(value, out var time) &&
               time > new TimeOnly(6, 0) &&
               time <= new TimeOnly(23, 0);
    }

    private static bool TryParseTime(string value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(
            value,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }
}
