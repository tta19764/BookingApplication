using BookingApp.Bll.Abstractions.Events;
using BookingApp.Bll.Common.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingApp.Bll.Bookings.CancelBooking;

/// <summary>
/// Handles post-cancellation application side effects.
/// </summary>
public class BookingCancelledDomainEventManager(
    ILogger<BookingCancelledDomainEventManager> logger) : IDomainEventManager<BookingCancelledDomainEvent>
{
    public Task Handle(BookingCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was cancelled",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
