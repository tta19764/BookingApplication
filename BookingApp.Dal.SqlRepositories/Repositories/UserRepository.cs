using BookingApp.Bll.Common.Users;
using BookingApp.Dal.SqlRepositories.Entities;
namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class UserRepository(ApplicationDbContext db, EntityChangeTracker tracker) : Repository<UserEntity, User>(db, tracker), IUserRepository
{
    protected override IQueryable<UserEntity> Ordered(IQueryable<UserEntity> q) => q.OrderBy(x => x.Id); protected override Guid GetEntityId(UserEntity x) => x.Id; protected override Guid GetModelId(User x) => x.Id;
    protected override UserEntity ToEntity(User x) => new() { Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, Email = x.Email, Roles = x.Roles.ToList() };
    protected override User ToModel(UserEntity x) => User.Create(x.Id, x.FirstName, x.LastName, x.Email); protected override void UpdateEntity(UserEntity e, User m) { }
}
