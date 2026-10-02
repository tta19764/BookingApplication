using BookingApp.Application.Abstractions.Events;
using BookingApp.Domain.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingApp.Application.Bookings.CancelBooking;

/// <summary>
/// Handles post-cancellation application side effects.
/// </summary>
public class BookingCancelledDomainEventHandler(
    ILogger<BookingCancelledDomainEventHandler> logger) : IDomainEventHandler<BookingCancelledDomainEvent>
{
    public Task Handle(BookingCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was cancelled",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
