using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.Shared.Events;

namespace BookingApp.Services.Web.Services.DomainEvents;

/// <summary>
/// Records booking creation facts for operational monitoring.
/// </summary>
public sealed class BookingCreatedEventHandler(ILogger<BookingCreatedEventHandler> logger)
    : IDomainEventHandler<BookingCreatedDomainEvent>
{
    public Task HandleAsync(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Booking {BookingId} created for hall {ConferenceHallId} with total {TotalPrice} {Currency}",
            domainEvent.BookingId,
            domainEvent.ConferenceHallId,
            domainEvent.TotalPrice,
            domainEvent.Currency);

        return Task.CompletedTask;
    }
}
