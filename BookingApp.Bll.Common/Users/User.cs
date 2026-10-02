using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;

namespace BookingApp.Bll.Common.Users;

public class User : Entity
{
    public User()
    {
    }

    public User(Guid id, FirstName firstName, LastName lastName, Email email) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public FirstName FirstName { get; set; } = null!;
    public LastName LastName { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
