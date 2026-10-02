# Infrastructure layer

`BookingApp.Dal.PostgreSQLRepositories` implements external and persistence concerns required by the other layers.

## Persistence

- `ApplicationDbContext` is the EF Core unit of work.
- PostgreSQL repositories implement hall, booking, and user repository contracts.
- DAL persistence entities inherit a shared `Entity` base that provides database identity only.
- Entity configurations map DAL entities and value objects, including monetary precision, UTC booking timestamps, amenity conversion, relationships, and deletion behavior.
- Migrations version the relational schema.
- Availability and overlap checks are translated into database queries.
- Report reads are paginated and deterministic.

## Operational services

- Services registers .NET `TimeProvider.System`; no clock implementation belongs to DAL.
- Quartz and `CompleteBookingsJob` live in Services and process bounded batches of expired reservations.
- The DAL exposes concrete implementations; dependency injection is composed centrally by `BookingApp.Services.Web`.

## Configuration

Development connection strings and job settings live in `BookingApp.Services.Web/appsettings.Development.json`; Docker Compose overrides host-specific values. Secrets should be supplied by deployment configuration rather than committed settings in production.

Real repository mappings and database behavior are covered by both integration test projects using temporary PostgreSQL Testcontainers.
