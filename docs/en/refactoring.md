# Layered architecture and ADO.NET migration

The earlier layered refactor replaced mediator handlers with managers and versioned MVC controllers. This migration replaces its EF/PostgreSQL persistence with ADO.NET and remote SQL Server stored procedures.

| Project | Responsibility |
| --- | --- |
| BookingApp.Services.Web | HTTP DTOs/controllers, middleware, composition, Swagger, events and Quartz jobs. |
| BookingApp.Bll.Common | Provider-independent business models, value objects, repository/manager contracts, results and events. |
| BookingApp.Bll | Validation, pricing, booking workflow and application read models. |
| BookingApp.Dal.SqlServerRepositories | SqlClient connections/commands, persistence entities, reader hydration, AutoMapper profiles and stored procedure scripts. |

```text
Services.Web -> Bll -> Bll.Common
     |                    ^
     +-> SqlServer DAL ---+
```

Controllers depend on managers; BLL depends on Common repository interfaces. The DAL registration extension creates concrete repositories. Provider types and SQL stay in DAL. DAL persistence entities and AutoMapper profiles isolate storage representations from Common business models. BLL/Services retain their AutoMapper profiles; generic tracked repositories and EF tracking are removed.

Async writes persist immediately. IUnitOfWork is removed. One reservation operation atomically inserts the booking and updates hall booking time; SQL Server hall locks serialize occupancy checks. Only Reserved bookings block a hall. Hall edits never overwrite last-booked time, and referenced hall deletion returns HTTP 409. Completion uses bounded guarded updates and actual committed counts. BLL retains validation/pricing, and event dispatch occurs after commit.

Startup initialization is optional and controlled by configuration; required scripts run before data seeding. Initial setup uses your code-first database followed by a generated SQL setup script; no deployment-tool project is included. Stored procedures and required reference data are included before generating that script. PostgreSQL migrations cannot be replayed on SQL Server; existing data requires a validated export/import. See the [setup guide](database-initialization.md) and [migration plan](ef-to-ado-net-migration-plan.md).

All maintained test suites and Postman assets remain under test/. SQL Server Testcontainers run production scripts with restricted runtime credentials. A separate architecture project verifies provider isolation, controller boundaries and persistence structure; web unit tests cover serialization, events and controller behavior; persistence tests cover concurrency, rollback, completion and permissions. Application deployment uses remote SQL Server; database containers are test-only.

## Implemented persistence refactoring

| Previous EF/PostgreSQL mechanism | Current ADO.NET / SQL Server mechanism |
| --- | --- |
| `ApplicationDbContext`, tracked entities and `SaveChangesAsync` | Scoped repositories invoke stored procedures asynchronously; successful writes are already persisted. |
| EF queries and navigation loading | Typed `SqlCommand` parameters, explicit result sets, `RowMapper` hydration and DAL AutoMapper conversion. |
| Generic repository / `IUnitOfWork` | Operation-specific Common interfaces and outcomes; write procedures own atomic transactions. |
| EF migrations and startup bootstrap | Embedded versioned T-SQL scripts followed by data-only seed methods through `IDatabaseSeeder`. |
| PostgreSQL Compose database | Configured remote SQL Server connection; disposable SQL Server containers for integration tests only. |

Single-key DAL entities inherit `Entity<TKey>`: booking, hall and user keys remain Guid, while role and permission keys remain int. Join entities retain composite keys. Currency remains a three-letter code. The seven application tables preserve the logical relationships; provider-specific types, SQL syntax, indexes and concurrency enforcement changed. This is a persistence implementation change, not an automatic transfer of existing PostgreSQL data.

## Initialization and seeding boundary

`Bll.Common/Initialization/IDatabaseSeeder` defines initialization, inspection and reference/demo seeding without SqlClient types. DAL implements the interface; Services registers it as scoped and coordinates startup through `StartupDataSeeder`. Controllers and BLL managers do not execute setup SQL.

When `DatabaseSeeding:Enabled=true`, startup awaits `InitializeAsync`, then `SeedReferenceDataAsync`, then `SeedDemoDataAsync` only if `IncludeDemoData=true`. This completes before HTTP requests and hosted background jobs start. Failures stop startup. With seeding disabled, the interface remains registered but is not resolved by the startup coordinator.

Scripts reside in `Initialization/Scripts`; `Database/` is ignored personal development material and excluded from application resources. The initializer journals scripts with checksums and skips unchanged applied versions. Data methods use a shared application lock and separate serializable transactions: reference seeding fills missing required rows, while demo seeding skips any nonempty hall catalog. Script initialization and the two seed operations are not one global transaction; a later failure preserves earlier committed steps, allowing a corrected restart to resume safely.

See [database initialization](database-initialization.md) for configuration, permissions, explicit calls and troubleshooting.
