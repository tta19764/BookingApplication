# BLL layer

`BookingApp.Bll` coordinates use cases while keeping HTTP and database details outside the managers.

## Structure

- Public feature contracts (`IBookingManager`, `IConferenceHallManager`, and `IReportManager`) are declared in `BookingApp.Bll.Common`.
- Their implementations live in `BookingApp.Bll`; the Services layer depends only on the Common contracts.
- Each feature has one service-style manager implementation whose methods implement the feature's use cases directly.
- Operation-specific input models are declared in Common and use strongly typed date and time values.
- FluentValidation validators live in BLL and validate input shape, ranges, identifiers, and supported values before a manager performs a use case.
- BLL managers depend on repository interfaces, `IUnitOfWork`, .NET `TimeProvider`, validators, and pricing abstractions.
- There is no request/handler dispatcher; Services calls manager contracts directly.
- Response records and the BLL AutoMapper profile expose stable application read models.
- Validators are discovered from the BLL assembly; manager and repository registrations remain explicit in the Services composition root.

## Booking workflow

The booking manager loads the hall, builds a `DateRange`, rejects past or overlapping requests, delegates price calculation to `IPricingManager`, creates the booking model, persists through the unit of work, and returns an immutable price breakdown.

Input validation and business rules are intentionally separate. Validators handle context-free input constraints. Managers handle rules that require application state, such as hall existence, booking overlap, supported hall amenities, and bookings in the past. Validation failures are translated to the shared application `ValidationException` so the Services middleware returns a consistent response.

## Reporting

The booking summary manager processes bookings in bounded pages, aggregates total revenue and counts, and groups results per hall without loading the entire table into memory.

Managers are isolated with mocks in `BookingApp.Bll.UnitTests` and exercised with real persistence in `BookingApp.Bll.IntegrationTests`.
