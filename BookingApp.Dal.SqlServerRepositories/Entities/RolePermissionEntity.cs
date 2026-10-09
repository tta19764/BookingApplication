namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Represents a role-permission link identified by the composite RoleId and PermissionId key.</summary>
public sealed class RolePermissionEntity
{
    /// <summary>Gets or sets the referenced role identifier.</summary>
    public int RoleId { get; set; }
    /// <summary>Gets or sets the referenced permission identifier.</summary>
    public int PermissionId { get; set; }
}
