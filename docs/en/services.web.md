# API layer

`BookingApp.Services.Web` is the HTTP entry point. It uses versioned ASP.NET Core MVC controllers under `/api/v1`, maps application results to a consistent response envelope, and publishes Swagger documentation in Development. Controllers depend only on manager interfaces and mapping services; they never access repositories directly.

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
- Invalid business input is returned as controlled HTTP problem details.
- Result-to-HTTP mapping for successful, not-found, and business-rule responses.
- Central exception handling prevents internal exception details from leaking to clients.
- Request-context logging through Serilog and Seq.
- HTTPS redirection in the middleware pipeline.
- Initial schema setup may use code first and a generated SQL script; enabled startup applies the embedded initialization scripts to an empty database; startup initialization applies required scripts before data seeding when enabled; it does not create the database itself.

The current API uses a seeded user. Production deployment should add authentication, authorization policies, secret management, rate limiting, and environment-specific trust/proxy configuration.

The reusable Postman collection under `test/Postman/` provides an additional manual workflow. Automated HTTP behavior is covered by `BookingApp.Services.Web.IntegrationTests`.

Services registers Common `IDatabaseSeeder` as scoped and awaits `StartupDataSeeder` before HTTP/scheduler startup when `DatabaseSeeding:Enabled=true`. `IncludeDemoData` controls optional halls. An optional `ConnectionStrings:Seeding` overrides the main database connection; absent/blank values reuse Database. Seeding requires SELECT/INSERT permissions; failures prevent startup. Enabled startup applies required schema/procedure/permission scripts first; unchanged journaled scripts are skipped.
