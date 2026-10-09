namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Represents a user-role link identified by the composite UserId and RoleId key.</summary>
public sealed class UserRoleEntity
{
    /// <summary>Gets or sets the referenced user identifier.</summary>
    public Guid UserId { get; set; }
    /// <summary>Gets or sets the referenced role identifier.</summary>
    public int RoleId { get; set; }
}
