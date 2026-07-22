# Booking Application

Booking Application is a conference hall booking API. It allows a client to manage halls, search available halls for a requested time range, create bookings with selected amenities, calculate rental prices, and read business summary reports.

The project is intentionally split into API, Application, Domain, and Infrastructure layers so the business rules stay independent from transport and persistence details.

## Business Features

- Create, update, delete, and read conference halls.
- Search available conference halls by date, start time, end time, and required capacity.
- Reserve a hall for the seeded system user.
- Calculate booking prices from hall hourly rate, selected amenities, and time-based pricing rules.
- Automatically complete finished bookings with a Quartz background job.
- Read booking revenue summary data for reporting.

## Pricing Rules

Bookings are calculated inside the domain layer. A booking must be within one calendar day and within supported rental hours. Booking and availability requests accept minute-level UTC times in `HH:mm` format, for example `10:40`.

| Time window | Rule |
| --- | --- |
| 06:00-09:00 | 10% discount |
| 09:00-12:00 | Base hourly price |
| 12:00-14:00 | 15% peak-hour surcharge |
| 14:00-18:00 | Base hourly price |
| 18:00-23:00 | 20% discount |

Partial hours are charged proportionally. For example, 20 minutes is billed as one third of the applicable hourly rate. If a booking crosses a pricing boundary, the price is split by boundary and each slice gets its own modifier.

Amenities are charged once per booking:

| Amenity | Value | Price |
| --- | ---: | ---: |
| Projector | 1 | 500 UAH |
| Wi-Fi | 2 | 300 UAH |
| Sound system | 3 | 700 UAH |

## Seeded Data

The API seeds one user with all roles and permissions. For now, endpoints execute booking requests for this user instead of using registration, login, or external identity.

Seeded user ID:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
```

Seeded halls:

| Hall | Capacity | Hourly rate |
| --- | ---: | ---: |
| Hall A | 50 | 2000 UAH |
| Hall B | 100 | 3500 UAH |
| Hall C | 30 | 1500 UAH |

Conference hall IDs are generated during seeding. Use the paginated halls endpoint to read the current IDs before creating a booking.

## Technologies

- .NET 10
- ASP.NET Core Minimal APIs
- ASP.NET API Versioning
- Swagger / OpenAPI
- MediatR
- FluentValidation
- Entity Framework Core
- PostgreSQL
- Quartz.NET
- Serilog
- Seq
- Docker Compose
- xUnit
- FluentAssertions
- NSubstitute
- Testcontainers for PostgreSQL-backed integration tests

## Architecture

The solution follows a Clean Architecture style.

| Project | Responsibility |
| --- | --- |
| `BookingApp.Domain` | Entities, value objects, enums, domain events, repository contracts, pricing rules, and business invariants. |
| `BookingApp.Application` | Commands, queries, handlers, validators, mapping, pipeline behaviors, and application abstractions. |
| `BookingApp.Infrastructure` | EF Core database context, repository implementations, entity configurations, unit of work, date/time provider, and Quartz jobs. |
| `BookingApp.Api` | Minimal API endpoints, API versioning, Swagger, exception handling, request logging, dependency wiring, migration execution, and seed data. |
| `BookingApp.Domain.UnitTests` | Focused tests for domain behavior. |
| `test/BookingApp.Application.UnitTests` | Application handler tests with mocked repositories, unit of work, pricing service, and date/time dependencies. |
| `test/BookingApp.Application.IntegrationTests` | Application integration tests that run the real API host and EF Core repositories against a PostgreSQL Testcontainer. |

The API is versioned with URL segments, currently under `/api/v1`. Swagger is generated for the available API versions.

## Configuration

The base [appsettings.json](BookingApp.Api/appsettings.json) contains only default ASP.NET logging and `AllowedHosts`.

Development-specific settings are in [appsettings.Development.json](BookingApp.Api/appsettings.Development.json):

- PostgreSQL connection string
- Quartz background job options
- Serilog Console sink
- Serilog Seq sink

Docker Compose also sets `ASPNETCORE_ENVIRONMENT=Development` by default and overrides container-specific values such as the database host and Seq URL.

The API applies EF Core migrations on startup and then seeds roles, permissions, one system user, and the initial halls.

## Run With Docker Compose

Prerequisites:

- Docker Desktop

Start the full development environment:

```powershell
docker compose up --build
```

This starts:

- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Seq UI: `http://localhost:8081`
- Seq ingestion endpoint: `http://localhost:5341`

The Compose file sets `ASPNETCORE_ENVIRONMENT=Development`, points the API to the PostgreSQL container, and points Serilog to the Seq container. Seq runs without authentication in this local development setup.

Open Swagger:

```text
http://localhost:8080/swagger
```

Seq is available at:

```text
http://localhost:8081
```

Stop containers:

```powershell
docker compose down
```

Stop containers and remove persisted database and Seq data:

```powershell
docker compose down -v
```

## Run Locally Without Docker

Prerequisites:

- .NET 10 SDK
- PostgreSQL running locally on port `5432`
- Optional: Seq running locally on `http://localhost:5341`

The local development connection string expects:

```text
Host=localhost;Port=5432;Database=booking_app;Username=postgres;Password=postgres
```

Run the API:

```powershell
dotnet run --project BookingApp.Api
```

Open Swagger from the URL printed by ASP.NET Core. If the app uses the default HTTP launch profile, Swagger is usually available at:

```text
http://localhost:5086/swagger
```

The API applies migrations and seeds required data on startup.

## Postman

Postman files are stored at solution level in [Postman](Postman):

- `BookingApp.Api.postman_collection.json`
- `BookingApp.Local.postman_environment.json`

Import both files into Postman and select the `BookingApp Local` environment. The collection creates a test hall, stores its ID in an environment variable, creates a booking, checks overlap validation, reads paginated data, and reads the booking summary report.

## API Endpoints

All endpoints are versioned under:

```text
/api/v1
```

Conference halls:

```http
POST   /api/v1/conference-halls
GET    /api/v1/conference-halls?page=1&pageSize=20
GET    /api/v1/conference-halls/{hallId}
PUT    /api/v1/conference-halls/{hallId}
DELETE /api/v1/conference-halls/{hallId}
GET    /api/v1/conference-halls/available?date=2026-09-01&startTime=10:40&endTime=14:00&capacity=50
```

Bookings:

```http
GET  /api/v1/bookings?page=1&pageSize=20
POST /api/v1/bookings
```

Reports:

```http
GET /api/v1/reports/bookings-summary
```

Example booking request:

```json
{
  "hallId": "<conference-hall-id-from-get-conference-halls>",
  "date": "2026-09-01",
  "startTime": "10:40",
  "endTime": "14:00",
  "amenities": [1, 2]
}
```

Example hall creation request:

```json
{
  "name": "Hall D",
  "capacity": 80,
  "hourlyRate": 2800,
  "currencyCode": "UAH",
  "amenities": [1, 2, 3]
}
```

Successful booking responses include a price breakdown:

- hall rental price for the selected period
- amenities surcharge
- total price
- currency

## Reports And Analytics

The current report endpoint returns a booking revenue summary:

```http
GET /api/v1/reports/bookings-summary
```

It includes total bookings, total revenue, and per-hall booking/revenue breakdown. The query reads bookings in pages instead of loading all rows into memory.

## Useful Commands

Restore and build:

```powershell
dotnet restore BookingApplicationSolution.sln
dotnet build BookingApplicationSolution.sln
```

Run tests:

```powershell
dotnet test BookingApplicationSolution.sln
```

The test suite includes:

- Domain unit tests for entities, value objects, domain events, and pricing rules.
- Application unit tests for command/query handlers such as creating halls, creating bookings, available halls, and booking summary aggregation.
- Application integration tests using `WebApplicationFactory<Program>` and a temporary PostgreSQL container to verify DI, EF Core mappings, repositories, migrations, seeding, and handler execution together.

Docker Desktop must be running for the application integration tests because they use Testcontainers.

Validate Docker Compose configuration:

```powershell
docker compose config
```

## Operational Notes

- Development logs are written to the console and to Seq.
- Finished reservations are completed by the Quartz `CompleteBookingsJob`.
- The booking summary query reads bookings in pages instead of loading all bookings at once.
- The API currently uses one seeded user with full permissions. Authentication and authorization are intentionally not part of the current scope.
- Booking period columns and audit fields use UTC timestamps.
- Current-time guards and automatic completion compare booking periods against `IDateTimeProvider.UtcNow`.
- The current scope is a development/test API. Production hardening would add authentication, authorization policies, stricter access control, and deployment-specific secret management.
