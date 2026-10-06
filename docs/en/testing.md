# Testing

Run the entire suite with:

```powershell
dotnet test BookingApplicationSolution.sln
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

Architecture tests verify provider-free BLL dependencies, AutoMapper profiles, controller boundaries, TimeOnly serialization, domain-event dispatch and the bounded completion job.

Both test projects link `test/Shared/SqlServerWebAppFactory.cs`. The fixture starts an ephemeral SQL Server, applies the production embedded scripts, creates a restricted runtime identity and disposes the host/container after tests. Scheduled completion is disabled in API fixtures. No remote application credentials are used.

The SQL Server 2022 image is pinned by digest. Override `BOOKINGAPP_TEST_SQL_IMAGE` to match the remote engine major version and align compatibility level/collation/isolation where needed. Allow time/resources for the first image pull; CI failures to start SQL Server are reported as test failures. Optional remote staging smoke tests separately validate TLS, authentication and network access. See [database deployment](../../BookingApp.Dal.SqlServerRepositories/Database/README.md).


The current suite has 64 tests: Common 1, BLL unit 27, BLL integration 22 and Web integration 14. Entity architecture checks cover Guid identity inheritance and prevent business/provider types in DAL entity properties. Database-folder artifacts are deliberately kept outside the migration commits; restore/provision those local artifacts before building the integration test bootstrap from such a checkout.
