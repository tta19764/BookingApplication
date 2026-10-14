# Booking Application

Booking Application is a versioned REST API for managing conference halls and reservations. Clients can maintain a hall catalog, search availability, book halls with optional services, receive time-based price calculations, and read revenue analytics.

The solution uses Services.Web, Bll, Bll.Common, and Dal.SqlServerRepositories layers. It is built with .NET 10, ASP.NET Core MVC, ADO.NET (Microsoft.Data.SqlClient), remote SQL Server stored procedures, Quartz, Swagger, Serilog, xUnit, and test-only SQL Server Testcontainers.

Українська: Booking Application — це REST API для керування конференц-залами та бронюваннями. Система підтримує каталог залів, пошук доступності, додаткові послуги, розрахунок вартості за часовими тарифами й аналітику доходу. Рішення побудоване за принципами layered architecture.

## Documentation / Документація

| Subject | English | Українська |
| --- | --- | --- |
| Project overview, business rules, setup, and API | [Overview](docs/en/overview.md) | [Огляд](docs/ua/overview.md) |
| Common layer | [Common](docs/en/bll.common.md) | [Рівень Common](docs/ua/bll.common.md) |
| BLL layer | [BLL](docs/en/bll.md) | [Рівень BLL](docs/ua/bll.md) |
| DAL layer | [DAL](docs/en/dal.sqlserverrepositories.md) | [Рівень DAL](docs/ua/dal.sqlserverrepositories.md) |
| Database initialization and seeding | [SQL Server setup](docs/en/database-initialization.md) | [Ініціалізація та дані](docs/ua/database-initialization.md) |
| Service layer | [Service](docs/en/services.web.md) | [Рівень Service](docs/ua/services.web.md) |
| Test projects and strategy | [Testing](docs/en/testing.md) | [Тестування](docs/ua/testing.md) |
| Features added beyond the assignment | [Extended features](docs/en/extended-features.md) | [Розширені можливості](docs/ua/extended-features.md) |
| Layered architecture refactoring | [Refactoring](docs/en/refactoring.md) | [Рефакторинг](docs/ua/refactoring.md) |

## Technologies and tools / Технології та інструменти

| Area | Technologies and purpose |
| --- | --- |
| Runtime and language | **.NET 10** and **C#** for the application and test projects. |
| Web API | **ASP.NET Core MVC controllers** for HTTP endpoints and **ASP.NET API Versioning** for `/api/v1`. |
| API documentation | **Swagger/OpenAPI** through Swashbuckle for interactive endpoint documentation. |
| Application flow | Feature-oriented **BLL managers** that expose service-style business operations. |
| Domain design | Common models, DAL persistence entities and DAL/BLL/HTTP AutoMapper profiles, async repositories, FluentValidation and Result-based errors. |
| Persistence | **ADO.NET**, **Microsoft.Data.SqlClient**, remote **SQL Server**, stored procedures and versioned T-SQL scripts. |
| Background work | **Quartz.NET** for automatically completing expired bookings. |
| Logging | **Serilog** for structured logging and **Seq** for local log collection and inspection. |
| Containers | Optional API/Seq Compose stack; SQL Server containers only for integration tests. |
| Automated testing | **xUnit**, **FluentAssertions**, **NSubstitute**, `WebApplicationFactory`, and **Testcontainers**. |
| Manual/API testing | **Postman** collection and environment under [`test/Postman`](test/Postman), ready to run once the database and required reference user are configured. |
| Dependency security | NuGet vulnerability auditing and a patched direct SSH.NET dependency used by Testcontainers. |

## Quick start / Швидкий старт

### Azure Key Vault secrets

The Azure Functions hourly Telegram report has its own configuration. See
[report settings and database prerequisites](docs/en/functions-report.md).

Configure `KeyVault:Uri` to load connection strings and bot tokens from Azure Key Vault.
See [local sign-in, secret names, and deployment setup](docs/en/key-vault.md).
Vault secrets override matching earlier configuration values when enabled.

### Telegram booking notifications

Create a bot with [@BotFather](https://t.me/BotFather), start a conversation with it or add it to the destination group/channel with permission to send messages, and obtain the destination chat ID (for example, from the Bot API `getUpdates` response after sending the bot a message). Notifications use the [Telegram.Bot library](https://github.com/TelegramBots/Telegram.Bot) and its `SendMessage` method.

Store local secrets in the ignored `BookingApp.Services.Web/appsettings.Development.json`. If it is missing in a new checkout, copy the secret-free example once:

```powershell
Copy-Item BookingApp.Services.Web/appsettings.Development.example.json BookingApp.Services.Web/appsettings.Development.json
```

In the local file, fill in `ConnectionStrings:Database` (and optionally `ConnectionStrings:Seeding`), `Telegram:BotToken`, and `Telegram:ChatId`; set `Telegram:Enabled` to `true`. The launch profiles select Development, so ASP.NET Core automatically loads this file over the base configuration:

```powershell
dotnet run --project BookingApp.Services.Web
```

Keep real secrets only in the local ignored file. It is excluded from Docker build contexts. Environment variables override JSON settings; containers and non-Development deployments should obtain secrets from their environment or deployment secret store. Ignored configuration is local to each checkout and is not restored by Git. Older branches that still track this filename can overwrite or remove it when switching branches; preserve a local backup before switching to those branches. Avoid `git clean -x`, which removes ignored files too.

Notifications are disabled by default. When enabled, missing token/chat settings fail startup validation. Each successfully persisted new reservation sends the hall name, customer name, start/end times in UTC, duration, total and reservation status to the configured chat. Set the optional `Telegram:MessageThreadId` to deliver notifications to a specific forum topic. Rejected bookings do not send messages. Delivery is best effort, runs during the creation request with a ten-second timeout, and has no retry or durable queue; Telegram/network failures are logged without failing a saved booking. Request cancellation can interrupt delivery after persistence. A durable outbox would be needed for guaranteed eventual delivery.

Prerequisites: .NET 10 and an approved remote SQL Server database. Docker is needed only for integration tests or optional API/Seq hosting.

Create an empty SQL Server application database, configure its connection string, and enable initialization to apply embedded schema/procedure/permission scripts before seeding data. The runner does not create the database itself. For an existing external code-first schema, establish a reviewed baseline first; the runner rejects unjournaled application tables. See [database initialization](docs/en/database-initialization.md). A separate EXECUTE-only runtime identity can be used after setup. No deployment-tool project is included. The DAL `DatabaseSeeder`, registered as `IDatabaseSeeder`, can inspect existing row counts and seed reference or demo data explicitly or at opt-in startup; see the [DAL guide](docs/en/dal.sqlserverrepositories.md#explicit-database-seeding).

```powershell
dotnet run --project BookingApp.Services.Web
```

For optional API/Seq containers, set `ConnectionStrings__Database` to the remote runtime connection string before running:

```powershell
docker compose up --build
```

- API and Swagger: `http://localhost:8080` and `http://localhost:8080/swagger`
- Seq: `http://localhost:8081`

Run all tests / Запуск усіх тестів:

```powershell
dotnet test --solution BookingApplicationSolution.sln
```

Integration tests start ephemeral SQL Server containers with the real scripts and restricted runtime credentials. They require Docker and do not connect to the remote application database. Startup initialization is opt-in through `DatabaseSeeding:Enabled` and defaults to false. When enabled, required scripts run before reference and optional demo data. Existing PostgreSQL data requires a deliberate export/import; deployment does not transfer it.

After initial database setup, for manual verification of the five required API methods, import the files from [`test/Postman`](test/Postman) and run the collection in order.

Opt-in startup data seeding is registered through Common `IDatabaseSeeder`. Set `DatabaseSeeding__Enabled=true` and optionally provide `ConnectionStrings__Seeding` for a separate identity. Missing or blank Seeding falls back to Database; the selected identity needs SELECT/INSERT access for data plus setup rights for unapplied scripts. `DatabaseSeeding__IncludeDemoData=true` optionally adds demo halls. Both flags default to false; failures prevent startup. The Compose API service forwards these settings. Enabled startup applies required schema/procedure/permission scripts first; unchanged journaled scripts are skipped.
