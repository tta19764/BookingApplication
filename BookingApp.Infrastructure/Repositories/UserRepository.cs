using BookingApp.Domain.Users;

namespace BookingApp.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for user persistence.
/// </summary>
public class UserRepository(ApplicationDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
}
