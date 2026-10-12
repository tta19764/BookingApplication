namespace BookingApp.Bll.Common.Shared.Exceptions;

/// <summary>
/// Single validation failure returned by the application validation pipeline.
/// </summary>
/// <param name="PropertyName">Name of the input property that failed validation.</param>
/// <param name="ErrorMessage">Description of the validation failure.</param>
public sealed record ValidationError(string PropertyName, string ErrorMessage)
{
    /// <summary>Name of the input property that failed validation.</summary>
    public string PropertyName { get; init; } = PropertyName;

    /// <summary>Description of the validation failure.</summary>
    public string ErrorMessage { get; init; } = ErrorMessage;
}
