using BookingApp.Bll.Common.Users;

namespace BookingApp.Dal.SqlRepositories.Repositories;

/// <summary>
/// EF Core repository for user persistence.
/// </summary>
public class UserRepository(ApplicationDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
}
