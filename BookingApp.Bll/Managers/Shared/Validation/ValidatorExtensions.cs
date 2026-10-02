using BookingApp.Bll.Common.Shared.Exceptions;
using FluentValidation;

namespace BookingApp.Bll.Managers.Shared.Validation;

internal static class ValidatorExtensions
{
    internal static async Task ValidateForApplicationAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        throw new BookingApp.Bll.Common.Shared.Exceptions.ValidationException(
            result.Errors.Select(failure => new ValidationError(failure.PropertyName, failure.ErrorMessage)));
    }
}
