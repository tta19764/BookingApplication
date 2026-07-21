using BookingApp.Domain.Bookings.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingApp.Application.Bookings.RejectBooking;

/// <summary>
/// Handles post-rejection application side effects.
/// </summary>
public class BookingRejectedDomainEventHandler(
    ILogger<BookingRejectedDomainEventHandler> logger) : INotificationHandler<BookingRejectedDomainEvent>
{
    public Task Handle(BookingRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was rejected",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
