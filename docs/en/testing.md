# Testing

Run the entire suite with:

```powershell
dotnet test --solution BookingApplicationSolution.sln
```

Docker Desktop or another compatible Docker engine is required for integration tests.

## BookingApp.Bll.Common.UnitTests

Fast tests for value objects and business-model behavior without mocks, HTTP, or a database.

## BookingApp.Bll.UnitTests

Manager and validator tests use NSubstitute and FluentAssertions. They verify asynchronous writes, atomic reservation outcomes, event dispatch only after success, persistence failures, pricing, validation and paginated reporting.

## BookingApp.Bll.IntegrationTests

BLL tests call managers and the production SqlClient repositories against SQL Server Testcontainers. They cover mapping/Unicode/UTC/money, status-aware occupancy, concurrent reservations, rollback of hall changes, role/permission hydration, guarded completion, migration reruns/checksums and EXECUTE-only permissions. A shared nonparallel test collection owns one container; tests use distinct data IDs.

## BookingApp.Services.Web.IntegrationTests

HTTP tests use HttpClient and a SQL Server container. They cover hall create/update/delete, availability and booking, HTTP statuses, JSON contracts and price totals.

Configuration integration tests verify AutoMapper profiles, SQL authentication providers and Quartz registration.

## BookingApp.ArchitectureTests

Structural checks verify provider-free BLL dependencies, controller boundaries, embedded SQL resources and persistence entity types. They are tagged `Category=Architecture` and run without Docker.

## BookingApp.Services.Web.UnitTests

Isolated behavioral tests cover controller error routing, middleware, request serialization, domain-event dispatch, startup seeding and completion-job execution. They run without Docker.

Both test projects link `test/Shared/SqlServerWebAppFactory.cs`. The fixture starts an ephemeral SQL Server, applies the Initialization scripts, invokes reference/demo seed methods and creates a restricted runtime identity and disposes the host/container after tests. Scheduled completion is disabled in API fixtures. No remote application credentials are used.

The SQL Server 2022 image is pinned by digest. Override `BOOKINGAPP_TEST_SQL_IMAGE` to match the remote engine major version and align compatibility level/collation/isolation where needed. Allow time/resources for the first image pull; CI failures to start SQL Server are reported as test failures. Optional remote staging smoke tests separately validate TLS, authentication and network access. See [database deployment](database-initialization.md).


Entity architecture checks cover Guid/integer typed identity inheritance and prevent business/provider types in DAL entity properties. The Database folder contains only personal files excluded from application scope. Test bootstrap uses Initialization scripts and data seeder methods.

`DatabaseSeederTests` creates a separate schema-only database for each test in the shared SQL Server container. It checks empty data inspection, missing-table SQL failures, existing data preservation, partial reference repair, conflicting identities, insert rollback, concurrent idempotency and rejection of runtime credentials. It never alters the shared application-test schema or the remote database.

StartupDataSeederTests verifies interface registration, disabled behavior without credentials, reference/demo ordering, optional connection fallback/override, failure propagation and cancellation without Docker. Fixtures disable startup seeding to keep data initialization under test control.

`RepositoryExceptionTests` exercises actual SQL Server failures through hall, booking and user repositories. Its 18 cases cover read/write errors, missing schema, command timeouts, connection failures and cancellation. It verifies provider-independent categories, sanitized exception contents, incident correlation and exactly one error log. Each case owns a disposable database; production procedure signatures are retained while test bodies inject failures.

`SqlObjectNamingTests` verifies fresh creation of all eight prefixed tables (including the journal), 14 procedures and five journaled scripts, without legacy schema, runtime role or tables.

The updated xUnit packages use Microsoft Testing Platform. Root `global.json` selects this runner for .NET 10. Use `dotnet test --solution BookingApplicationSolution.sln`; a class filter uses `-- --filter-class "*CompleteBookingsJobTests"`. Completion job tests verify the Quartz 4 cancellation parameter, one cutoff, bounded batching, failure propagation and the registered job/trigger interval.
