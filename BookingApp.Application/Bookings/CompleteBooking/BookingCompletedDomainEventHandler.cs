using BookingApp.Domain.Bookings.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingApp.Application.Bookings.CompleteBooking;

/// <summary>
/// Handles post-completion application side effects.
/// </summary>
public class BookingCompletedDomainEventHandler(
    ILogger<BookingCompletedDomainEventHandler> logger) : INotificationHandler<BookingCompletedDomainEvent>
{
    public Task Handle(BookingCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was completed",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
