namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores an integer-keyed role and its explicitly hydrated permissions.</summary>
public sealed class RoleEntity
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the stored name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the role permissions; the repository loads them explicitly.</summary>
    public ICollection<PermissionEntity> Permissions { get; set; } = [];
}
