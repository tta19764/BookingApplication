namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores an integer-keyed permission catalog entry.</summary>
public sealed class PermissionEntity
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the stored name.</summary>
    public string Name { get; set; } = string.Empty;
}
