namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Provides a Guid primary key for booking, hall and user persistence entities.</summary>
public abstract class Entity
{
    /// <summary>Gets or sets the primary key.</summary>
    public Guid Id { get; set; }
}
