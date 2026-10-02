# Domain layer

`BookingApp.Bll.Common` contains plain business models and contracts that remain independent of ASP.NET Core and EF Core. Persistence entities belong exclusively to `BookingApp.Dal.PostgreSQLRepositories`.

## Responsibilities

- Every feature folder groups its `Models`, `Exceptions`, manager interface, and repository interface.

- `ConferenceHall`, `Booking`, and `User` are anemic business models and do not inherit from a persistence entity base.
- Public manager, repository, pricing, and unit-of-work interfaces define the boundaries implemented by BLL and DAL projects.
- Value objects such as `Money`, `Currency`, `Name`, `Capacity`, and `DateRange` keep invalid primitive combinations out of business logic.
- `IPricingManager` is declared in Common, while the BLL `PricingManager` implements tariff boundaries, modifiers, and amenity pricing.
- Repository and unit-of-work interfaces define persistence needs without choosing a database.
- `Result` and domain error catalogs represent expected business failures explicitly.

## Important invariants

- A booking period has a valid start and end and uses UTC timestamps in persistence.
- Pricing accepts a single calendar day between 06:00 and 23:00 at minute precision.
- Only amenities supported by the selected hall can be purchased.
- A booking can only transition from an appropriate current status.
- Persisted booking prices are snapshots and do not change when hall pricing changes later.

The layer is verified directly by `BookingApp.Bll.Common.UnitTests`; see [Testing](testing.md).
