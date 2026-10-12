namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>
/// Booking confirmation returned after a successful reservation, including the price breakdown.
/// </summary>
/// <param name="BookingId">Identifier of the created booking.</param>
/// <param name="HallId">Identifier of the conference hall.</param>
/// <param name="Start">Start of the rental period in UTC.</param>
/// <param name="End">End of the rental period in UTC.</param>
/// <param name="PriceForPeriod">Hall rental cost for the requested period.</param>
/// <param name="AmenitiesUpCharge">Total charge for selected amenities.</param>
/// <param name="TotalPrice">Combined rental and amenity cost.</param>
/// <param name="Currency">Currency code for the monetary amounts.</param>
public sealed record BookingConfirmationModel(
    Guid BookingId,
    Guid HallId,
    DateTime Start,
    DateTime End,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency)
{
    /// <summary>Identifier of the created booking.</summary>
    public Guid BookingId { get; init; } = BookingId;

    /// <summary>Identifier of the conference hall.</summary>
    public Guid HallId { get; init; } = HallId;

    /// <summary>Start of the rental period in UTC.</summary>
    public DateTime Start { get; init; } = Start;

    /// <summary>End of the rental period in UTC.</summary>
    public DateTime End { get; init; } = End;

    /// <summary>Hall rental cost for the requested period.</summary>
    public decimal PriceForPeriod { get; init; } = PriceForPeriod;

    /// <summary>Total charge for selected amenities.</summary>
    public decimal AmenitiesUpCharge { get; init; } = AmenitiesUpCharge;

    /// <summary>Combined rental and amenity cost.</summary>
    public decimal TotalPrice { get; init; } = TotalPrice;

    /// <summary>Currency code for the monetary amounts.</summary>
    public string Currency { get; init; } = Currency;
}
