# API layer

`BookingApp.Api` is the HTTP entry point. It uses ASP.NET Core Minimal APIs under `/api/v1`, maps application results to a consistent response envelope, and publishes Swagger documentation in Development.

## Endpoints

```http
POST   /api/v1/conference-halls
GET    /api/v1/conference-halls?page=1&pageSize=20
GET    /api/v1/conference-halls/{hallId}
PUT    /api/v1/conference-halls/{hallId}
DELETE /api/v1/conference-halls/{hallId}
GET    /api/v1/conference-halls/available?date=2026-09-01&startTime=10:40&endTime=14:00&capacity=50

GET    /api/v1/bookings?page=1&pageSize=20
POST   /api/v1/bookings

GET    /api/v1/reports/bookings-summary
```

## Cross-cutting behavior

- URL-segment API versioning and versioned Swagger documents.
- FluentValidation failures returned as HTTP 400 problem details.
- Result-to-HTTP mapping for successful, not-found, and business-rule responses.
- Central exception handling prevents internal exception details from leaking to clients.
- Request-context logging through Serilog and Seq.
- HTTPS redirection in the middleware pipeline.
- Development migrations and idempotent seed-data initialization.

The current API uses a seeded user. Production deployment should add authentication, authorization policies, secret management, rate limiting, and environment-specific trust/proxy configuration.

The reusable Postman collection under `test/Postman/` provides an additional manual workflow. Automated HTTP behavior is covered by `BookingApp.Api.IntegrationTests`.
