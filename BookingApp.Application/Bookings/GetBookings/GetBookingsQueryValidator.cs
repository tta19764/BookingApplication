using FluentValidation;

namespace BookingApp.Application.Bookings.GetBookings;

/// <summary>
/// Validates pagination values for booking list queries.
/// </summary>
public sealed class GetBookingsQueryValidator : AbstractValidator<GetBookingsQuery>
{
    public GetBookingsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
