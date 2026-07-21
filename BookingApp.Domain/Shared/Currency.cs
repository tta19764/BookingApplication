namespace BookingApp.Domain.Shared;

/// <summary>
/// Supported currency code used by prices in the booking domain.
/// </summary>
public record Currency
{
    internal static readonly Currency None = new("");
    /// <summary>
    /// Ukrainian hryvnia.
    /// </summary>
    public static readonly Currency Uah = new("UAH");

    private Currency(string code) => Code = code;

    public string Code { get; init; }

    /// <summary>
    /// Resolves a supported currency by code.
    /// </summary>
    public static Currency FromCode(string code)
    {
        return All.FirstOrDefault(c => c.Code == code) ??
               throw new ApplicationException("The currency code is invalid");
    }

    /// <summary>
    /// All currencies accepted by the domain.
    /// </summary>
    public static readonly IReadOnlyCollection<Currency> All =
    [
        Uah
    ];
}