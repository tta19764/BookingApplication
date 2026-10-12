namespace BookingApp.Bll.Common.ConferenceHalls.Models;

/// <summary>
/// Amenity data exposed to API consumers, including its fixed price.
/// </summary>
/// <param name="Type">Amenity category.</param>
/// <param name="Name">Display name of the hall or amenity.</param>
/// <param name="Price">Fixed amenity charge.</param>
/// <param name="Currency">Currency code for the monetary amounts.</param>
public sealed record AmenityModel(
    Amenity Type,
    string Name,
    decimal Price,
    string Currency)
{
    /// <summary>Amenity category.</summary>
    public Amenity Type { get; init; } = Type;

    /// <summary>Display name of the hall or amenity.</summary>
    public string Name { get; init; } = Name;

    /// <summary>Fixed amenity charge.</summary>
    public decimal Price { get; init; } = Price;

    /// <summary>Currency code for the monetary amounts.</summary>
    public string Currency { get; init; } = Currency;
}

/// <summary>
/// Conference hall read model used by hall queries.
/// </summary>
/// <param name="Id">Unique identifier of this item.</param>
/// <param name="Name">Display name of the hall or amenity.</param>
/// <param name="Capacity">Number of seats required or provided.</param>
/// <param name="HourlyRate">Hall rental price per hour.</param>
/// <param name="Currency">Currency code for the monetary amounts.</param>
/// <param name="Amenities">Amenities included in the hall or requested for the booking.</param>
public sealed record HallModel(
    Guid Id,
    string Name,
    int Capacity,
    decimal HourlyRate,
    string Currency,
    IReadOnlyCollection<AmenityModel> Amenities)
{
    /// <summary>Unique identifier of this item.</summary>
    public Guid Id { get; init; } = Id;

    /// <summary>Display name of the hall or amenity.</summary>
    public string Name { get; init; } = Name;

    /// <summary>Number of seats required or provided.</summary>
    public int Capacity { get; init; } = Capacity;

    /// <summary>Hall rental price per hour.</summary>
    public decimal HourlyRate { get; init; } = HourlyRate;

    /// <summary>Currency code for the monetary amounts.</summary>
    public string Currency { get; init; } = Currency;

    /// <summary>Amenities included in the hall or requested for the booking.</summary>
    public IReadOnlyCollection<AmenityModel> Amenities { get; init; } = Amenities;
}