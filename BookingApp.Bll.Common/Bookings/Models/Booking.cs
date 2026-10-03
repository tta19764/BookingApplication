using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Users.Models;

namespace BookingApp.Bll.Common.Bookings.Models;

public class Booking
{
    public Booking()
    {
    }

    public Booking(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
    public Guid ConferenceHallId { get; set; }
    public Guid UserId { get; set; }
    public DateRange Duration { get; set; } = null!;
    public Money PriceForPeriod { get; set; } = null!;
    public Money AmenitiesUpCharge { get; set; } = null!;
    public Money TotalPrice { get; set; } = null!;
    public BookingStatus Status { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? RejectedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? CancelledOnUtc { get; set; }
    public ConferenceHall ConferenceHall { get; set; } = null!;
    public User User { get; set; } = null!;
}
