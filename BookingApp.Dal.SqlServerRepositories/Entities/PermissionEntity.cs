namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores an integer-keyed permission catalog entry.</summary>
public sealed class PermissionEntity : Entity<int>
{
    /// <summary>Gets or sets the stored name.</summary>
    public string Name { get; set; } = string.Empty;
}
