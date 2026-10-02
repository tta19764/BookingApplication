using BookingApp.Bll.Common.Users;

namespace BookingApp.Dal.SqlRepositories.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }
    public FirstName FirstName { get; set; } = null!;
    public LastName LastName { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<BookingEntity> Bookings { get; set; } = [];
}
