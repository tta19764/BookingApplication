namespace BookingApp.Domain.Shared;

/// <summary>
/// Monetary amount paired with its currency.
/// </summary>
public record Money(decimal Amount, Currency Currency)
{
    /// <summary>
    /// Adds two monetary values when they use the same currency.
    /// </summary>
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return new Money(first.Amount + second.Amount, first.Currency);
    }

    /// <summary>
    /// Creates a zero amount without a real currency.
    /// </summary>
    public static Money Zero() => new(0, Currency.None);

    /// <summary>
    /// Creates a zero amount for the specified currency.
    /// </summary>
    public static Money Zero(Currency currency) => new(0, currency);

    /// <summary>
    /// Returns true when the amount is zero in the current currency.
    /// </summary>
    public bool IsZero() => this == Zero(Currency);
}