using BookingApp.Bll.Abstractions.Events;
using BookingApp.Bll.Common.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingApp.Bll.Bookings.AddBooking;

/// <summary>
/// Handles post-reservation application side effects.
/// </summary>
public class BookingReservedDomainEventManager(
    ILogger<BookingReservedDomainEventManager> logger) : IDomainEventManager<BookingReservedDomainEvent>
{
    public Task Handle(BookingReservedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was reserved",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
