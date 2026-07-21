using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;

namespace BookingApp.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = [];
    
    private User(Guid id, FirstName firstName, LastName lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    private User()
    {
    }

    public FirstName FirstName { get; private set; }

    public LastName LastName { get; private set; }

    public Email Email { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.ToList();
    
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    
    public static User Create(FirstName firstName, LastName lastName, Email email)
    {
        var user = new User(Guid.NewGuid(), firstName, lastName, email);

        user._roles.Add(Role.Registered);
        
        return user;
    }
}