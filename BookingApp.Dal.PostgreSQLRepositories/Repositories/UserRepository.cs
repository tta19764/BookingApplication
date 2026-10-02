using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Dal.SqlRepositories.Entities;
using AutoMapper;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext, IMapper mapper)
    : Repository<UserEntity, User>(dbContext, mapper), IUserRepository
{
    protected override IQueryable<UserEntity> Ordered(IQueryable<UserEntity> query) =>
        query.OrderBy(user => user.Id);

    protected override Guid GetModelId(User model) => model.Id;
}
