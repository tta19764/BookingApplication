namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores booking relationships, UTC lifecycle timestamps and calculated price snapshots.</summary>
public sealed class BookingEntity : Entity<Guid>
{
    /// <summary>Gets or sets the referenced hall identifier.</summary>
    public Guid ConferenceHallId { get; set; }
    /// <summary>Gets or sets the referenced user identifier.</summary>
    public Guid UserId { get; set; }
    /// <summary>Gets or sets the inclusive UTC rental start.</summary>
    public DateTime Start { get; set; }
    /// <summary>Gets or sets the exclusive UTC rental end.</summary>
    public DateTime End { get; set; }
    /// <summary>Gets or sets the persisted PriceForPeriod amount.</summary>
    public decimal PriceForPeriodAmount { get; set; }
    /// <summary>Gets or sets the three-letter PriceForPeriod currency code.</summary>
    public string PriceForPeriodCurrency { get; set; } = string.Empty;
    /// <summary>Gets or sets the persisted AmenitiesUpCharge amount.</summary>
    public decimal AmenitiesUpChargeAmount { get; set; }
    /// <summary>Gets or sets the three-letter AmenitiesUpCharge currency code.</summary>
    public string AmenitiesUpChargeCurrency { get; set; } = string.Empty;
    /// <summary>Gets or sets the persisted TotalPrice amount.</summary>
    public decimal TotalPriceAmount { get; set; }
    /// <summary>Gets or sets the three-letter TotalPrice currency code.</summary>
    public string TotalPriceCurrency { get; set; } = string.Empty;
    /// <summary>Gets or sets the stored booking status name.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC created timestamp.</summary>
    public DateTime CreatedOnUtc { get; set; }
    /// <summary>Gets or sets the nullable UTC rejected timestamp.</summary>
    public DateTime? RejectedOnUtc { get; set; }
    /// <summary>Gets or sets the nullable UTC completed timestamp.</summary>
    public DateTime? CompletedOnUtc { get; set; }
    /// <summary>Gets or sets the nullable UTC cancelled timestamp.</summary>
    public DateTime? CancelledOnUtc { get; set; }
}
