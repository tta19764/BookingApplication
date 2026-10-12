namespace BookingApp.Bll.Common.Shared;

/// <summary>
/// Domain or application error code and message returned by failed results.
/// </summary>
/// <param name="Code">Stable error code identifying the failed business rule.</param>
/// <param name="Name">Human-readable description of the failure.</param>
public record Error(string Code, string Name)
{
    /// <summary>Stable error code identifying the failed business rule.</summary>
    public string Code { get; init; } = Code;

    /// <summary>Human-readable description of the failure.</summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Empty error used by successful results.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Error used when a required value is missing.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");
}