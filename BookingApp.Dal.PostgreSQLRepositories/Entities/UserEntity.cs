using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;

namespace BookingApp.Dal.SqlRepositories.Entities;

public sealed class UserEntity : Entity
{
    public FirstName FirstName { get; set; } = null!;
    public LastName LastName { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<BookingEntity> Bookings { get; set; } = [];
}
