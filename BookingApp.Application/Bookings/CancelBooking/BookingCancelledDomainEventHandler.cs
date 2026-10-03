using BookingApp.Domain.Bookings.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingApp.Application.Bookings.CancelBooking;

/// <summary>
/// Handles post-cancellation application side effects.
/// </summary>
public class BookingCancelledDomainEventHandler(
    ILogger<BookingCancelledDomainEventHandler> logger) : INotificationHandler<BookingCancelledDomainEvent>
{
    public Task Handle(BookingCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was cancelled",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
