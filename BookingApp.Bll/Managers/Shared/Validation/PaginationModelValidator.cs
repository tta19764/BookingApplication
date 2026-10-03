using BookingApp.Bll.Common.Shared.Models;
using FluentValidation;

namespace BookingApp.Bll.Managers.Shared.Validation;

public sealed class PaginationModelValidator : AbstractValidator<PaginationModel>
{
    public PaginationModelValidator()
    {
        RuleFor(model => model.Page)
            .GreaterThan(0);

        RuleFor(model => model.PageSize)
            .InclusiveBetween(1, 100);
    }
}
