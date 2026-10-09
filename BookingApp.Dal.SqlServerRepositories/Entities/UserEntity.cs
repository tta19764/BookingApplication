namespace BookingApp.Dal.SqlServerRepositories.Entities;

/// <summary>Stores user values and explicitly hydrated role relationships.</summary>
public sealed class UserEntity : Entity<Guid>
{
    /// <summary>Gets or sets the stored first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the stored last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the stored email address.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the user roles; the repository loads them explicitly.</summary>
    public ICollection<RoleEntity> Roles { get; set; } = [];
}
