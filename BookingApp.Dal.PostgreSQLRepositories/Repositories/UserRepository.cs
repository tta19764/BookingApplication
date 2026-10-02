using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Dal.SqlRepositories.Entities;
using AutoMapper;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext, IMapper mapper)
    : Repository<UserEntity, User>(dbContext, mapper), IUserRepository
{
}
