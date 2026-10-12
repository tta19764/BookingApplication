using BookingApp.Bll.Common.Users.Models;

namespace BookingApp.Bll.Common.Users;

/// <summary>
/// Provides persistence operations for users.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by its identifier.</summary>
    /// <param name="id">Identifier of the requested user.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The matching item, or null when it does not exist.</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists the user and its initial role assignments atomically.</summary>
    /// <param name="user">User and initial role assignments to persist.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>A task completed after the item is persisted.</returns>
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
