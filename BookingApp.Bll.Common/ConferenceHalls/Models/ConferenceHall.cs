using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.ConferenceHalls;

public class ConferenceHall
{
    public ConferenceHall()
    {
    }

    public ConferenceHall(Guid id, Name name, Capacity seats, Money price, List<Amenity> amenities)
    {
        Id = id;
        Name = name;
        Seats = seats;
        Price = price;
        Amenities = amenities;
    }

    public Guid Id { get; set; }
    public Name Name { get; set; } = null!;
    public Capacity Seats { get; set; } = null!;
    public Money Price { get; set; } = null!;
    public DateTime? LastBookedOnUtc { get; set; }
    public List<Amenity> Amenities { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
