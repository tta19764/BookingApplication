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

Development startup creates Hall A (50 seats, 2000 UAH/hour), Hall B (100, 3500), Hall C (30, 1500), and the seeded user `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`. Authentication is outside the current scope, so booking endpoints use this user.

## Running

With Docker Desktop running:

```powershell
docker compose up --build
```

This exposes the API on `http://localhost:8080`, Swagger on `/swagger`, PostgreSQL on `localhost:5432`, and Seq on `http://localhost:8081`.

For local execution, provide PostgreSQL at `localhost:5432` and run:

```powershell
dotnet run --project BookingApp.Api
```

## Architecture

Dependencies point inward: API and Infrastructure depend on Application and Domain, while Domain contains no transport or persistence concerns. Continue with the [Domain](domain.md), [Application](application.md), [Infrastructure](infrastructure.md), [API](api.md), and [Testing](testing.md) documents.

