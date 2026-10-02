using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Users;

namespace BookingApp.Bll.Common.Bookings;

public class Booking : Entity
{
    protected Booking()
    {
    }

    protected Booking(
        Guid id,
        Guid conferenceHallId,
        Guid userId,
        DateRange duration,
        Money priceForPeriod,
        Money amenitiesUpCharge,
        Money totalPrice,
        BookingStatus status,
        DateTime createdOnUtc)
        : base(id)
    {
        ConferenceHallId = conferenceHallId;
        UserId = userId;
        Duration = duration;
        PriceForPeriod = priceForPeriod;
        AmenitiesUpCharge = amenitiesUpCharge;
        TotalPrice = totalPrice;
        Status = status;
        CreatedOnUtc = createdOnUtc;
    }

    public Guid ConferenceHallId { get; private set; }

    public Guid UserId { get; private set; }

    public DateRange Duration { get; private set; } = null!;

    public Money PriceForPeriod { get; private set; } = null!;

    public Money AmenitiesUpCharge { get; private set; } = null!;

    public Money TotalPrice { get; private set; } = null!;

    public BookingStatus Status { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime? RejectedOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }

    public DateTime? CancelledOnUtc { get; private set; }

    public ConferenceHall ConferenceHall { get; private set; } = null!;

    public User User { get; private set; } = null!;

    public static Booking Reserve(
        ConferenceHall hall,
        IEnumerable<Amenity>? amenities,
        Guid userId,
        DateRange duration,
        DateTime utcNow,
        PricingService pricingService)
    {
        // Reservation owns price calculation so persisted bookings keep an immutable price snapshot.
        var pricingDetails = pricingService.CalculatePrice(hall, duration, amenities);

        var booking = new Booking(
            Guid.NewGuid(),
            hall.Id,
            userId,
            duration,
            pricingDetails.PriceForPeriod,
            pricingDetails.AmenitiesUpCharge,
            pricingDetails.TotalPrice,
            BookingStatus.Reserved,
            utcNow);

        booking.RaiseDomainEvent(new BookingReservedDomainEvent(booking.Id));

        // Keep the hall's operational metadata in sync with the successful reservation.
        hall.LastBookedOnUtc = utcNow;

        return booking;
    }

    public Result Reject(DateTime utcNow)
    {
        if (Status != BookingStatus.Reserved)
        {
            return Result.Failure(BookingErrors.NotReserved);
        }

        Status = BookingStatus.Rejected;
        RejectedOnUtc = utcNow;

        RaiseDomainEvent(new BookingRejectedDomainEvent(Id));

        return Result.Success();
    }

    public Result Complete(DateTime utcNow)
    {
        if (Status != BookingStatus.Reserved)
        {
            return Result.Failure(BookingErrors.NotReserved);
        }

        Status = BookingStatus.Completed;
        CompletedOnUtc = utcNow;

        RaiseDomainEvent(new BookingCompletedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status != BookingStatus.Reserved)
        {
            return Result.Failure(BookingErrors.NotReserved);
        }

        if (utcNow > Duration.Start)
        {
            // Started bookings must move through completion/rejection rules, not cancellation.
            return Result.Failure(BookingErrors.AlreadyStarted);
        }

        Status = BookingStatus.Cancelled;
        CancelledOnUtc = utcNow;

        RaiseDomainEvent(new BookingCancelledDomainEvent(Id));

        return Result.Success();
    }

    public static Booking Restore(
        Guid id,
        Guid conferenceHallId,
        Guid userId,
        DateRange duration,
        Money priceForPeriod,
        Money amenitiesUpCharge,
        Money totalPrice,
        BookingStatus status,
        DateTime createdOnUtc,
        DateTime? rejectedOnUtc,
        DateTime? completedOnUtc,
        DateTime? cancelledOnUtc)
    {
        return new Booking(id, conferenceHallId, userId, duration, priceForPeriod,
            amenitiesUpCharge, totalPrice, status, createdOnUtc)
        {
            RejectedOnUtc = rejectedOnUtc,
            CompletedOnUtc = completedOnUtc,
            CancelledOnUtc = cancelledOnUtc
        };
    }
}
