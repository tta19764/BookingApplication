using BookingApp.Bll.Common.Users;
using BookingApp.Dal.SqlRepositories.Entities;
using AutoMapper;
namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class UserRepository(ApplicationDbContext db, EntityChangeTracker tracker, IMapper mapper) : Repository<UserEntity, User>(db, tracker, mapper), IUserRepository
{
    protected override IQueryable<UserEntity> Ordered(IQueryable<UserEntity> q) => q.OrderBy(x => x.Id); protected override Guid GetEntityId(UserEntity x) => x.Id; protected override Guid GetModelId(User x) => x.Id;
}
