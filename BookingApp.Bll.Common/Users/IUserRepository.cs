using BookingApp.Bll.Common.Users.Models;

namespace BookingApp.Bll.Common.Users;

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
    /// Persists the user and its initial role assignments atomically.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
