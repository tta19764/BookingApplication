# Application layer

`BookingApp.Bll` coordinates use cases while keeping HTTP and database details outside the managers.

## Structure

- Public feature contracts (`IBookingManager`, `IConferenceHallManager`, and `IReportManager`) are declared in `BookingApp.Bll.Common`.
- Their implementations live in `BookingApp.Bll`; the Services layer depends only on the Common contracts.
- Each feature has one service-style manager implementation whose methods implement the feature's use cases directly.
- BLL managers depend on repository interfaces, `IUnitOfWork`, `IDateTimeProvider`, and pricing abstractions.
- There is no request/handler dispatcher; Services calls manager contracts directly.
- Response records and mappers expose stable application read models.
- AutoMapperConfig centralizes mapping between Common models, DAL entities, and Services DTOs.

## Booking workflow

The booking manager loads the hall, builds a `DateRange`, rejects past or overlapping requests, delegates price calculation to `IPricingManager`, creates the booking model, persists through the unit of work, and returns an immutable price breakdown.

## Reporting

The booking summary manager processes bookings in bounded pages, aggregates total revenue and counts, and groups results per hall without loading the entire table into memory.

Managers are isolated with mocks in `BookingApp.Bll.UnitTests` and exercised with real persistence in `BookingApp.Bll.IntegrationTests`.
