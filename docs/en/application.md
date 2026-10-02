# Application layer

`BookingApp.Bll` coordinates use cases while keeping HTTP and database details outside the managers.

## Structure

- manager requests mutate state: add/update/remove a hall and create or change a booking.
- manager requests return hall lists, availability, bookings, and the booking summary report.
- BLL managers depend on domain repositories, `IUnitOfWork`, `IDateTimeProvider`, and domain services.
- FluentValidation validators reject malformed IDs, paging, capacity, currency, amenity, and time inputs before managers execute.
- Manager dispatch provides centralized validation and structured request logging.
- Response records and mappers expose stable application read models.
- Domain event managers host post-operation side effects such as event logging.

## Booking workflow

The booking manager loads the hall, builds a `DateRange`, rejects past or overlapping requests, delegates price calculation and reservation creation to the Domain layer, persists through the unit of work, and returns an immutable price breakdown.

## Reporting

The booking summary manager processes bookings in bounded pages, aggregates total revenue and counts, and groups results per hall without loading the entire table into memory.

Managers are isolated with mocks in `BookingApp.Bll.UnitTests` and exercised with real persistence in `BookingApp.Bll.IntegrationTests`.

