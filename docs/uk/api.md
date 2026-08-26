# Рівень API

`BookingApp.Api` є HTTP entry point. Він використовує ASP.NET Core Minimal APIs під `/api/v1`, перетворює application results на єдиний response envelope і публікує Swagger у Development.

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

## Наскрізна поведінка

- Версіонування через URL і окремі Swagger documents для версій.
- FluentValidation повертає HTTP 400 problem details.
- Мапінг result у success, not-found та business-rule HTTP responses.
- Централізована обробка винятків не розкриває клієнтам внутрішні деталі.
- Request-context logging через Serilog і Seq.
- HTTPS redirection у middleware pipeline.
- Development migrations та ідемпотентний seed data startup.

Поточне API використовує seeded-користувача. Для production слід додати authentication, authorization policies, secret management, rate limiting та конфігурацію proxy/trust для середовища.

Багаторазова Postman collection у `test/Postman/` надає ручний сценарій. Автоматичну HTTP-поведінку перевіряє `BookingApp.Api.IntegrationTests`.
