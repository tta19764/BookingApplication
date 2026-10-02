using BookingApp.Bll.Common.Bookings.Models;
using FluentValidation;

namespace BookingApp.Bll.Managers.Bookings.Validation;

public sealed class CreateBookingModelValidator : AbstractValidator<CreateBookingModel>
{
    public CreateBookingModelValidator()
    {
        RuleFor(model => model.HallId).NotEmpty();
        RuleFor(model => model.UserId).NotEmpty();
        RuleFor(model => model.Date).NotEmpty();
        RuleFor(model => model.StartTime)
            .InclusiveBetween(new TimeOnly(6, 0), new TimeOnly(22, 59));
        RuleFor(model => model.EndTime)
            .InclusiveBetween(new TimeOnly(6, 1), new TimeOnly(23, 0))
            .GreaterThan(model => model.StartTime);
        RuleFor(model => model.Amenities).NotNull();
        RuleForEach(model => model.Amenities).IsInEnum();
    }
}
