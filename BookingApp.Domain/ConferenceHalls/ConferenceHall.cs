using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.Shared;

namespace BookingApp.Domain.ConferenceHalls;

public sealed class ConferenceHall : Entity
{
    private ConferenceHall()
    {
    }

    public ConferenceHall(Guid id, Name name, Capacity seats, Money price, List<Amenity> amenities) : base(id)
    {
        Name = name;
        Seats = seats;
        Price = price;
        Amenities = amenities;
    }

    public Name Name { get; private set; } = null!;
    public Capacity Seats { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public DateTime? LastBookedOnUtc { get; internal set; }

    public List<Amenity> Amenities { get; private set; } = [];
    
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    public void Update(Name name, Capacity seats, Money price, IEnumerable<Amenity> amenities)
    {
        Name = name;
        Seats = seats;
        Price = price;
        Amenities = amenities.Distinct().ToList();
    }

    public bool SupportsAmenity(Amenity amenity)
    {
        return Amenities.Contains(amenity);
    }
}
