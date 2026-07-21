namespace BookingApp.Domain.Abstractions;

/// <summary>
/// Domain or application error code and message returned by failed results.
/// </summary>
public record Error(string Code, string Name)
{
    /// <summary>
    /// Empty error used by successful results.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Error used when a required value is missing.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");
}