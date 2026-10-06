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

Web startup does not migrate or seed the remote database. Initial setup uses your code-first database followed by a generated SQL setup script; no deployment-tool project is included. Stored procedures and required reference data are included before generating that script. PostgreSQL migrations cannot be replayed on SQL Server; existing data requires a validated export/import. See the [setup guide](../../BookingApp.Dal.SqlServerRepositories/Database/README.md) and [migration plan](ef-to-ado-net-migration-plan.md).

All four maintained test suites and Postman assets remain under test/. SQL Server Testcontainers run production scripts with restricted runtime credentials. Architecture tests verify provider isolation, mappings, controller boundaries, serialization and events; persistence tests cover concurrency, rollback, completion and permissions. Application deployment uses remote SQL Server; database containers are test-only.
