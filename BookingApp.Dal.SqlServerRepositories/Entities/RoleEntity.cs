namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores an integer-keyed role and its explicitly hydrated permissions.</summary>
public sealed class RoleEntity : Entity<int>
{
    /// <summary>Gets or sets the stored name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the role permissions; the repository loads them explicitly.</summary>
    public ICollection<PermissionEntity> Permissions { get; set; } = [];
}
