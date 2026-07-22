using FluentValidation;

namespace BookingApp.Application.ConferenceHalls.GetHalls;

/// <summary>
/// Validates pagination values for conference hall list queries.
/// </summary>
public sealed class GetHallsQueryValidator : AbstractValidator<GetHallsQuery>
{
    public GetHallsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
