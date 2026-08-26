# Booking Application

Booking Application is a versioned REST API for managing conference halls and reservations. Clients can maintain a hall catalog, search availability, book halls with optional services, receive time-based price calculations, and read revenue analytics.

The solution uses Clean Architecture with separate Domain, Application, Infrastructure, and API layers. It is built with .NET 10, ASP.NET Core Minimal APIs, EF Core, PostgreSQL, MediatR, FluentValidation, Quartz, Swagger, Serilog, xUnit, and Testcontainers.

Українська: Booking Application — це REST API для керування конференц-залами та бронюваннями. Система підтримує каталог залів, пошук доступності, додаткові послуги, розрахунок вартості за часовими тарифами й аналітику доходу. Рішення побудоване за принципами Clean Architecture.

## Documentation / Документація

| Subject | English | Українська |
| --- | --- | --- |
| Project overview, business rules, setup, and API | [Overview](docs/en/overview.md) | [Огляд](docs/uk/overview.md) |
| Domain layer | [Domain](docs/en/domain.md) | [Доменний рівень](docs/uk/domain.md) |
| Application layer | [Application](docs/en/application.md) | [Прикладний рівень](docs/uk/application.md) |
| Infrastructure layer | [Infrastructure](docs/en/infrastructure.md) | [Інфраструктурний рівень](docs/uk/infrastructure.md) |
| API layer | [API](docs/en/api.md) | [Рівень API](docs/uk/api.md) |
| Test projects and strategy | [Testing](docs/en/testing.md) | [Тестування](docs/uk/testing.md) |
| Features added beyond the assignment | [Extended features](docs/en/extended-features.md) | [Розширені можливості](docs/uk/extended-features.md) |

## Technologies and tools / Технології та інструменти

| Area | Technologies and purpose |
| --- | --- |
| Runtime and language | **.NET 10** and **C#** for the application and test projects. |
| Web API | **ASP.NET Core Minimal APIs** for HTTP endpoints and **ASP.NET API Versioning** for `/api/v1`. |
| API documentation | **Swagger/OpenAPI** through Swashbuckle for interactive endpoint documentation. |
| Application flow | **MediatR** for commands, queries, handlers, pipeline behaviors, and domain-event dispatch. |
| Validation | **FluentValidation** for centralized request and command validation. |
| Domain design | Clean Architecture, entities, value objects, domain events, repository abstractions, and Result-based errors. |
| Persistence | **Entity Framework Core**, **Npgsql**, **PostgreSQL**, and EF Core migrations. |
| Background work | **Quartz.NET** for automatically completing expired bookings. |
| Logging | **Serilog** for structured logging and **Seq** for local log collection and inspection. |
| Containers | **Docker** and **Docker Compose** for the API, PostgreSQL, and Seq development stack. |
| Automated testing | **xUnit**, **FluentAssertions**, **NSubstitute**, `WebApplicationFactory`, and **Testcontainers**. |
| Manual/API testing | **Postman** collection and environment under [`test/Postman`](test/Postman), ready to run without editing data. |
| Dependency security | NuGet vulnerability auditing and a patched direct SSH.NET dependency used by Testcontainers. |

## Quick start / Швидкий старт

Prerequisite / Передумова: Docker Desktop.

```powershell
docker compose up --build
```

- API and Swagger: `http://localhost:8080` and `http://localhost:8080/swagger`
- Seq: `http://localhost:8081`
- PostgreSQL: `localhost:5432`

Run all tests / Запуск усіх тестів:

```powershell
dotnet test BookingApplicationSolution.sln
```

The integration test projects start temporary PostgreSQL containers and therefore require a working Docker engine.

For an out-of-the-box manual verification of the five required API methods, import the files from [`test/Postman`](test/Postman) and run the collection in order.
