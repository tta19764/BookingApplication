# Testing

All test methods use explicit `Arrange`, `Act`, and `Assert` sections. Run the entire suite with:

```powershell
dotnet test BookingApplicationSolution.sln
```

Docker Desktop or another compatible Docker engine is required for integration tests.

## BookingApp.Domain.UnitTests

Fast tests for booking state transitions, domain events, supported hours, partial-hour calculations, tariff-boundary splitting, discounts, peak surcharges, amenity prices, and invalid periods. They do not use mocks, HTTP, or a database.

## BookingApp.Application.UnitTests

Handler-focused tests using NSubstitute. They verify hall creation, availability querying, booking success and failures, overlap handling, unsupported amenities, past dates, repository calls, unit-of-work calls, and paginated report aggregation.

## BookingApp.Application.IntegrationTests

Application-level tests resolve MediatR and EF Core from the real host, execute commands/queries without HTTP, and verify PostgreSQL persistence, mappings, seeded data, UTC timestamps, availability, and price breakdowns.

## BookingApp.Api.IntegrationTests

End-to-end tests call the in-memory ASP.NET Core host through `HttpClient` while using a real temporary PostgreSQL database. They cover the five required operations: create, update, delete, availability search, and booking. Assertions include HTTP status codes, routing/model binding, JSON contracts, persistence, overlap exclusion, and exact peak-hour/amenity totals.

Each integration test project owns its `WebApplicationFactory` and PostgreSQL container so API and application test concerns remain separate.

