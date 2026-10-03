# Features beyond the assignment

The assignment requires hall creation, editing, deletion, availability search, booking, pricing rules, documentation, security considerations, and useful reports. The solution adds the following supporting capabilities to make that core usable and extensible.

## Users, roles, and permissions

The Domain and Infrastructure layers contain `User`, `Role`, `Permission`, and `RolePermission` models, repositories, and EF Core mappings. Development seeding creates a stable system user and associates it with the Registered role and hall/booking read/write permissions.

The booking endpoint currently assigns reservations to this seeded user because registration and authentication were not part of the requested API methods. These models prepare the solution for later identity integration, but they do **not** currently enforce endpoint authorization. Production work should add authentication and permission policies before treating the API as multi-user secure.

## Read endpoints and pagination

In addition to the five required mutations/search operations, the API exposes hall details, paginated hall lists, and paginated booking lists. These endpoints support administration, allow clients to discover generated IDs, and prevent unbounded collection responses.

## Reports and analytics

`GET /api/v1/reports/bookings-summary` returns total booking count, total revenue, currency, and count/revenue grouped by hall. The application processes bookings in bounded pages rather than loading the complete table into memory.

## Booking lifecycle automation

Bookings have explicit Reserved, Rejected, Completed, and Cancelled states with domain events. A Quartz background job finds expired reserved bookings in bounded batches and completes them automatically.

## Versioning and API documentation

URL-based versioning (`/api/v1`) allows future contracts to coexist. Swagger/OpenAPI is generated per API version in Development, and the root URL redirects to Swagger.

## Validation and error handling

FluentValidation and MediatR pipeline behavior centralize input checks. Application/domain failures use Result objects, while API middleware turns validation and unexpected failures into controlled responses and avoids leaking stack traces.

## Observability

Serilog adds structured request and application logs. Docker Compose includes Seq so developers can search and inspect logs without additional setup. Domain-event handlers log booking lifecycle events.

## Seed data and repeatable startup

Development startup applies migrations and idempotently creates roles, permissions, the seeded user, and the three required halls. Stable user identity and generated hall IDs make local scenarios predictable without hard-coding database rows in clients.

## Expanded verification

The solution separates Domain unit, Application unit, Application integration, and API integration tests. PostgreSQL Testcontainers validate real mappings and queries. The collection in `test/Postman/` dynamically generates future dates and unique names, chains IDs, and asserts all five required API operations repeatedly without manual data changes.

## Operational foundation

Docker Compose supplies the API, PostgreSQL, and Seq. Configuration is split by environment, timestamps are stored in UTC, and `IDateTimeProvider` makes time-dependent behavior testable.

