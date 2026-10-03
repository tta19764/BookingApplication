using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.Bll.Common.Users.Models;

public class User
{
    public User()
    {
    }

    public User(Guid id, FirstName firstName, LastName lastName, Email email)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public Guid Id { get; set; }
    public FirstName FirstName { get; set; } = null!;
    public LastName LastName { get; set; } = null!;
    public Email Email { get; set; } = null!;
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
