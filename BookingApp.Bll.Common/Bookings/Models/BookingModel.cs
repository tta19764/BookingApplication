namespace BookingApp.Bll.Common.Bookings.Models;

/// <summary>
/// Booking read model used by paginated booking queries.
/// </summary>
/// <param name="Id">Unique identifier of this item.</param>
/// <param name="HallId">Identifier of the conference hall.</param>
/// <param name="UserId">Identifier of the booking owner.</param>
/// <param name="Start">Start of the rental period in UTC.</param>
/// <param name="End">End of the rental period in UTC.</param>
/// <param name="Status">Current booking status.</param>
/// <param name="PriceForPeriod">Hall rental cost for the requested period.</param>
/// <param name="AmenitiesUpCharge">Total charge for selected amenities.</param>
/// <param name="TotalPrice">Combined rental and amenity cost.</param>
/// <param name="Currency">Currency code for the monetary amounts.</param>
public sealed record BookingModel(
    Guid Id,
    Guid HallId,
    Guid UserId,
    DateTime Start,
    DateTime End,
    string Status,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency)
{
    /// <summary>Unique identifier of this item.</summary>
    public Guid Id { get; init; } = Id;

    /// <summary>Identifier of the conference hall.</summary>
    public Guid HallId { get; init; } = HallId;

    /// <summary>Identifier of the booking owner.</summary>
    public Guid UserId { get; init; } = UserId;

    /// <summary>Start of the rental period in UTC.</summary>
    public DateTime Start { get; init; } = Start;

    /// <summary>End of the rental period in UTC.</summary>
    public DateTime End { get; init; } = End;

    /// <summary>Current booking status.</summary>
    public string Status { get; init; } = Status;

    /// <summary>Hall rental cost for the requested period.</summary>
    public decimal PriceForPeriod { get; init; } = PriceForPeriod;

    /// <summary>Total charge for selected amenities.</summary>
    public decimal AmenitiesUpCharge { get; init; } = AmenitiesUpCharge;

    /// <summary>Combined rental and amenity cost.</summary>
    public decimal TotalPrice { get; init; } = TotalPrice;

    /// <summary>Currency code for the monetary amounts.</summary>
    public string Currency { get; init; } = Currency;
}
