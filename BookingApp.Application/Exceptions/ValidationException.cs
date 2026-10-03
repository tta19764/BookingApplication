namespace BookingApp.Application.Exceptions;

/// <summary>
/// Exception thrown when application request validation fails.
/// </summary>
public sealed class ValidationException(IEnumerable<ValidationError> errors) : Exception
{
    public IEnumerable<ValidationError> Errors { get; } = errors;
}