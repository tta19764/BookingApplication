namespace BookingApp.Domain.Users;

/// <summary>
/// Provides persistence operations for users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the persistence context.
    /// </summary>
    void Add(User user);
}