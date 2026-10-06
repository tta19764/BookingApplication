# Project overview

## Business purpose

Booking Application manages the rental of conference halls. It lets clients create, update, delete, and inspect halls; search for a hall by date, time, and capacity; reserve it with selected amenities; calculate the rental price; and retrieve booking revenue analytics.

## Main features

- Hall catalog management with name, capacity, hourly price, currency, and supported amenities.
- Availability search that excludes overlapping reserved bookings.
- Booking confirmation with hall cost, amenity surcharge, total, and currency.
- Paginated hall and booking queries.
- Booking summary report with total and per-hall revenue.
- Automatic completion of expired reservations through Quartz.
- Swagger/OpenAPI documentation and URL-based API versioning.

## Pricing

Bookings must stay within one calendar day and the supported `06:00–23:00` rental window. Partial hours are charged proportionally, and periods crossing a tariff boundary are split into independently priced slices.

| Time | Modifier |
| --- | ---: |
| 06:00–09:00 | 10% discount |
| 09:00–12:00 | Base rate |
| 12:00–14:00 | 15% surcharge |
| 14:00–18:00 | Base rate |
| 18:00–23:00 | 20% discount |

Amenities are charged once per booking: Projector `500 UAH`, Wi-Fi `300 UAH`, and Sound system `700 UAH`.

## Seed data

Reference SQL installs the user `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` while authentication is outside scope. Optional `004_demo_seed.sql` creates Hall A (50 seats, 2000 UAH/hour), Hall B (100, 3500), and Hall C (30, 1500). Web startup does not seed data.

## Running

Deploy the [SQL Server scripts](../../BookingApp.Dal.SqlServerRepositories/Database/README.md) to an approved remote database and set `ConnectionStrings__Database` with runtime credentials. For optional API/Seq hosting with Docker:

```powershell
docker compose up --build
```

This exposes API on `http://localhost:8080`, Swagger on `/swagger`, and Seq on `http://localhost:8081`. The database remains remote SQL Server.

For local execution with the remote connection configured, run:

```powershell
dotnet run --project BookingApp.Services.Web
```

## Architecture

Common contains business/read models, DAL readers hydrate persistence entities and AutoMapper converts them to business models, and Services owns HTTP DTOs. Only DAL knows the provider. Async writes call stored procedures directly; atomic reservation operations replace EF tracking and IUnitOfWork.

`Services.Web` is the composition root and references every layer explicitly so all dependency injection registrations remain at the application boundary. Continue with the [Common](bll.common.md), [BLL](bll.md), [DAL](dal.sqlserverrepositories.md), [Service](services.web.md), [Testing](testing.md), and [Refactoring](refactoring.md) documents.
