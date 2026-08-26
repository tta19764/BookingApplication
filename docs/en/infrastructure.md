# Infrastructure layer

`BookingApp.Infrastructure` implements external and persistence concerns required by the inner layers.

## Persistence

- `ApplicationDbContext` is the EF Core unit of work.
- PostgreSQL repositories implement hall, booking, and user repository contracts.
- Entity configurations map aggregates and value objects, including monetary precision, UTC booking timestamps, amenity conversion, relationships, and deletion behavior.
- Migrations version the relational schema.
- Availability and overlap checks are translated into database queries.
- Report reads are paginated and deterministic.

## Operational services

- `DateTimeProvider` supplies UTC time behind an application abstraction.
- Quartz schedules `CompleteBookingsJob`, which processes bounded batches of expired reservations.
- Dependency injection registration is centralized in the layer's `DependencyInjection` class.

## Configuration

Development connection strings and job settings live in `BookingApp.Api/appsettings.Development.json`; Docker Compose overrides host-specific values. Secrets should be supplied by deployment configuration rather than committed settings in production.

Real repository mappings and database behavior are covered by both integration test projects using temporary PostgreSQL Testcontainers.

