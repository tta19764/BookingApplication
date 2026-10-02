using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Dal.SqlRepositories.Entities;

public sealed class BookingEntity : Entity
{
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
    public ConferenceHallEntity ConferenceHall { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}
