# Testing

All test methods use explicit `Arrange`, `Act`, and `Assert` sections. Run the entire suite with:

```powershell
dotnet test BookingApplicationSolution.sln
```

Docker Desktop or another compatible Docker engine is required for integration tests.

## BookingApp.Bll.Common.UnitTests

Fast tests for value objects and business-model behavior without mocks, HTTP, or a database.

## BookingApp.Bll.UnitTests

Manager and validator tests using NSubstitute and FluentAssertions. They verify hall creation, booking success and missing-hall behavior, pricing rules, input validation, repository and unit-of-work calls, event dispatch, and paginated report aggregation.

## BookingApp.Bll.IntegrationTests

BLL-level tests resolve managers and EF Core from the real host, call manager methods without HTTP, and verify PostgreSQL persistence, mappings, seeded data, UTC timestamps, availability, and price breakdowns.

## BookingApp.Services.Web.IntegrationTests

End-to-end tests call the in-memory ASP.NET Core host through `HttpClient` while using a real temporary PostgreSQL database. They cover the five required operations: create, update, delete, availability search, and booking. Assertions include HTTP status codes, routing/model binding, JSON contracts, persistence, overlap exclusion, and exact peak-hour/amenity totals.

Architecture tests in this project also validate all AutoMapper profiles, DAL entity inheritance, controller-to-manager boundaries, `TimeOnly` request serialization, and domain-event dispatch behavior.

Each integration test project owns its `WebApplicationFactory` and PostgreSQL container so API and application test concerns remain separate.

