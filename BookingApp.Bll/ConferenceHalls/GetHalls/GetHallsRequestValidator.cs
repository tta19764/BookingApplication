using FluentValidation;

namespace BookingApp.Bll.ConferenceHalls.GetHalls;

/// <summary>
/// Validates pagination values for conference hall list queries.
/// </summary>
public sealed class GetHallsRequestValidator : AbstractValidator<GetHallsRequest>
{
    public GetHallsRequestValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
