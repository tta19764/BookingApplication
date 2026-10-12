namespace BookingApp.Bll.Common.ConferenceHalls.Models;

/// <summary>Input for creating a conference hall.</summary>
/// <param name="Name">Display name of the hall or amenity.</param>
/// <param name="Capacity">Number of seats required or provided.</param>
/// <param name="HourlyRate">Hall rental price per hour.</param>
/// <param name="CurrencyCode">Currency code for the hourly rate.</param>
/// <param name="Amenities">Amenities included in the hall or requested for the booking.</param>
public sealed record CreateHallModel(
    string Name,
    int Capacity,
    decimal HourlyRate,
    string CurrencyCode,
    IReadOnlyCollection<Amenity> Amenities)
{
    /// <summary>Display name of the hall or amenity.</summary>
    public string Name { get; init; } = Name;

    /// <summary>Number of seats required or provided.</summary>
    public int Capacity { get; init; } = Capacity;

    /// <summary>Hall rental price per hour.</summary>
    public decimal HourlyRate { get; init; } = HourlyRate;

    /// <summary>Currency code for the hourly rate.</summary>
    public string CurrencyCode { get; init; } = CurrencyCode;

    /// <summary>Amenities included in the hall or requested for the booking.</summary>
    public IReadOnlyCollection<Amenity> Amenities { get; init; } = Amenities;
}
