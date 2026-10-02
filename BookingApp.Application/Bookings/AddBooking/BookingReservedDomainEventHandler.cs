using BookingApp.Application.Abstractions.Events;
using BookingApp.Domain.Bookings.Events;
using Microsoft.Extensions.Logging;

namespace BookingApp.Application.Bookings.AddBooking;

/// <summary>
/// Handles post-reservation application side effects.
/// </summary>
public class BookingReservedDomainEventHandler(
    ILogger<BookingReservedDomainEventHandler> logger) : IDomainEventHandler<BookingReservedDomainEvent>
{
    public Task Handle(BookingReservedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} was reserved",
            notification.BookingId);

        return Task.CompletedTask;
    }
}
