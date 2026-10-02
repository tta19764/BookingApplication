using BookingApp.Bll.Abstractions.Events;
using BookingApp.Bll.Common.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingApp.Bll.Bookings.CompleteBooking;

/// <summary>
/// Handles post-completion application side effects.
/// </summary>
public class BookingCompletedDomainEventManager(
    ILogger<BookingCompletedDomainEventManager> logger) : IDomainEventManager<BookingCompletedDomainEvent>
{
    public Task Handle(BookingCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was completed",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
