using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.Shared;

namespace BookingApp.Domain.ConferenceHalls;

public sealed class ConferenceHall(Guid id, Name name, Capacity seats, Money price, List<Amenity> amenities)
    : Entity(id)
{
    public Name Name { get; private set; } = name;
    public Capacity Seats { get; private set; } = seats;
    public Money Price { get; private set; } = price;
    public DateTime? LastBookedOnUtc { get; internal set; }

    public List<Amenity> Amenities { get; private set; } = amenities;
    
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    public bool SupportsAmenity(Amenity amenity)
    {
        return Amenities.Contains(amenity);
    }
}