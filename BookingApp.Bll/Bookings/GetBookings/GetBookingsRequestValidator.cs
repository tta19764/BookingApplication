using FluentValidation;

namespace BookingApp.Bll.Bookings.GetBookings;

/// <summary>
/// Validates pagination values for booking list queries.
/// </summary>
public sealed class GetBookingsRequestValidator : AbstractValidator<GetBookingsRequest>
{
    public GetBookingsRequestValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
