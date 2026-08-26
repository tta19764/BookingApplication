# Domain layer

`BookingApp.Domain` contains the business model and rules that must remain independent of ASP.NET Core and EF Core.

## Responsibilities

- `ConferenceHall` owns its name, capacity, hourly price, amenities, and booking metadata.
- `Booking` controls reservation creation and state transitions: Reserved, Rejected, Completed, and Cancelled.
- Value objects such as `Money`, `Currency`, `Name`, `Capacity`, and `DateRange` keep invalid primitive combinations out of business logic.
- `PricingService` splits a booking at tariff boundaries, applies modifiers, and adds supported amenities.
- Domain events describe reservation and status changes without coupling entities to side effects.
- Repository and unit-of-work interfaces define persistence needs without choosing a database.
- `Result` and domain error catalogs represent expected business failures explicitly.

## Important invariants

- A booking period has a valid start and end and uses UTC timestamps in persistence.
- Pricing accepts a single calendar day between 06:00 and 23:00 at minute precision.
- Only amenities supported by the selected hall can be purchased.
- A booking can only transition from an appropriate current status.
- Persisted booking prices are snapshots and do not change when hall pricing changes later.

The layer is verified directly by `BookingApp.Domain.UnitTests`; see [Testing](testing.md).

