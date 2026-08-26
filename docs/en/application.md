# Application layer

`BookingApp.Application` coordinates use cases while keeping HTTP and database details outside the handlers.

## Structure

- Commands mutate state: add/update/remove a hall and create or change a booking.
- Queries return hall lists, availability, bookings, and the booking summary report.
- MediatR handlers depend on domain repositories, `IUnitOfWork`, `IDateTimeProvider`, and domain services.
- FluentValidation validators reject malformed IDs, paging, capacity, currency, amenity, and time inputs before handlers execute.
- Pipeline behaviors provide centralized validation and structured request logging.
- Response records and mappers expose stable application read models.
- Domain event handlers host post-operation side effects such as event logging.

## Booking workflow

The create-booking handler loads the hall, builds a `DateRange`, rejects past or overlapping requests, delegates price calculation and reservation creation to the Domain layer, persists through the unit of work, and returns an immutable price breakdown.

## Reporting

The booking summary query processes bookings in bounded pages, aggregates total revenue and counts, and groups results per hall without loading the entire table into memory.

Handlers are isolated with mocks in `BookingApp.Application.UnitTests` and exercised with real persistence in `BookingApp.Application.IntegrationTests`.

