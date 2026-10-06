namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores hall catalog values and the reservation-managed last-booked timestamp.</summary>
public sealed class ConferenceHallEntity : Entity
{
    /// <summary>Gets or sets the stored name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the seating capacity.</summary>
    public int Capacity { get; set; }
    /// <summary>Gets or sets the hourly rental amount.</summary>
    public decimal HourlyRate { get; set; }
    /// <summary>Gets or sets the three-letter currency code.</summary>
    public string Currency { get; set; } = string.Empty;
    /// <summary>Gets or sets comma-separated numeric amenity values.</summary>
    public string Amenities { get; set; } = string.Empty;
    /// <summary>Gets or sets the nullable UTC timestamp maintained by reservation creation.</summary>
    public DateTime? LastBookedOnUtc { get; set; }
}
