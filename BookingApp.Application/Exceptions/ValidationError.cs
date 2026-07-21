namespace BookingApp.Application.Exceptions;

/// <summary>
/// Single validation failure returned by the application validation pipeline.
/// </summary>
public sealed record ValidationError(string PropertyName, string ErrorMessage);