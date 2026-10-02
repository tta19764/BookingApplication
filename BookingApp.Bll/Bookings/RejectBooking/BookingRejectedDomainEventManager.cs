using BookingApp.Bll.Abstractions.Events;
using BookingApp.Bll.Common.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingApp.Bll.Bookings.RejectBooking;

/// <summary>
/// Handles post-rejection application side effects.
/// </summary>
public class BookingRejectedDomainEventManager(
    ILogger<BookingRejectedDomainEventManager> logger) : IDomainEventManager<BookingRejectedDomainEvent>
{
    public Task Handle(BookingRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was rejected",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
