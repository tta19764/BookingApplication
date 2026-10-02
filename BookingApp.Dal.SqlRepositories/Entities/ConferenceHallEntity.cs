using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Dal.SqlRepositories.Entities;

public sealed class ConferenceHallEntity
{
    public Guid Id { get; set; }
    public Name Name { get; set; } = null!;
    public Capacity Seats { get; set; } = null!;
    public Money Price { get; set; } = null!;
    public DateTime? LastBookedOnUtc { get; set; }
    public List<Amenity> Amenities { get; set; } = [];
    public ICollection<BookingEntity> Bookings { get; set; } = [];
}
