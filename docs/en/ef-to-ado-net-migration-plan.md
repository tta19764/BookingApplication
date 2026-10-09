# EF Core / PostgreSQL to ADO.NET / remote SQL Server: migration plan

Status: the SQL Server DAL and container integration tests are implemented on the ADO migration branch based on `refactor/layered-architecture-v2`. Application changes are organized into functional commits; `BookingApp.Dal.SqlServerRepositories/Database/` is intentionally excluded from those commits. Schema/procedure/permission scripts and their runner now belong to Initialization; the Database folder is personal-only and outside application scope. The deployment-tool project and its source files are removed. Initial setup will use a code-first database followed by a generated SQL setup script. The local development database has been initialized and seeded through the startup flow. Existing-data transfer and production verification remain environment-specific steps.

This document retains the migration design and operational acceptance gates. The DAL guide describes the implemented API; remote verification, optional data transfer, monitoring and cutover remain deployment work. Recommendations below are not a claim that each operational enhancement is implemented.

## 1. Objective and confirmed constraints

Replace Entity Framework Core and PostgreSQL completely with direct ADO.NET using `Microsoft.Data.SqlClient` and stored procedures hosted on a remote Microsoft SQL Server database. Connect through a configured SQL Server connection string. Remove the PostgreSQL database service, its volume/health checks and PostgreSQL test fixtures. Integration tests use ephemeral SQL Server containers through `Testcontainers.MsSql`; application deployment uses remote SQL Server.

Retain .NET 10, the layered architecture and existing HTTP contracts. Keep input validation, period construction, pricing and application orchestration in the BLL. Use SQL Server stored procedures for both reads and writes; SQL Server procedures can return result sets directly, so read functions/refcursors are unnecessary. Microsoft recommends [Microsoft.Data.SqlClient for new SQL Server development](https://learn.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace?view=sql-server-ver17).

DAL isolation means:

1. BLL and controllers depend on application interfaces/models. Only DAL knows SQL Server commands, procedure names, result schemas, provider errors and transaction objects.
2. The runtime database user can execute approved procedures but cannot directly access application tables or alter the database schema. A separate deployment identity applies schema changes.

Remote server details are implementation inputs: hostname/instance and port, database name, SQL Server version and compatibility level, authentication method, certificate requirements, network access and deployment permissions. Identify a matching SQL Server test image and database settings. A remote test database is optional for staging smoke tests, not required for the integration suite. Do not infer these from the existing PostgreSQL configuration. A server URL alone may not contain everything required for a valid connection string.

Whether existing PostgreSQL data needs transfer remains a scope decision. If no data is required, install an empty SQL Server schema and seed baseline data. If data is required, perform the validated transfer in Section 8. No PostgreSQL dependency remains in the final application or test workflow; a one-time source export may be needed for existing data.

## 2. Pre-migration baseline and affected files

| Area | Pre-migration behavior | Migration change |
|---|---|---|
| DAL project | `BookingApp.Dal.PostgreSQLRepositories`, namespace `BookingApp.Dal.SqlRepositories` | Rename project/folder to `BookingApp.Dal.SqlServerRepositories`; update solution/project references and namespace references consistently. |
| Persistence | `ApplicationDbContext` implements `IUnitOfWork.SaveChangesAsync`; generic repository uses DbSet/tracking/AutoMapper | Replace with SqlClient repositories, explicit mappings and asynchronous write contracts. |
| Booking creation | Hall read, overlap check, price calculation, hall timestamp update, insert, save, event dispatch | Preserve atomic persistence and add database-side concurrency protection. |
| Occupancy | Booking precheck considers all statuses; availability considers only Reserved | Agree one blocking-status policy and apply it everywhere. |
| Background completion | Select due reservations, update individually, save batches | Use an atomic guarded completion procedure or transactional guarded writes. |
| Reporting | Booking pages of 500 aggregated by `ReportManager` | Preserve initially; database aggregation is optional later work. |
| Bootstrap | Development startup invokes EF migrations and EF seeding/EnsureCreated | Replace with opt-in script initialization followed by interface-based reference/demo data seeding; external code-first setup and generated personal scripts remain a separate workflow. |
| Schema | Seven PostgreSQL application tables and PostgreSQL EF migrations | Create a T-SQL schema; old provider-specific migrations are historical references only. |
| Tests | Two PostgreSQL Testcontainers factories and direct DbContext assertions | Replace with Testcontainers.MsSql and isolated SQL Server container fixtures. |
| Compose | `compose.yaml` defines PostgreSQL service, connection string, health check and volume | Remove database service and dependencies. Any retained API container connects to the remote server. |
| Documentation | README and English/Ukrainian DAL/testing docs describe PostgreSQL and Docker database prerequisites | Document remote runtime configuration and test-only Docker prerequisites. |

Review anchors: DAL context, repositories, formatters, initial migration and mappings; BLL booking/hall/report managers; web DI, Program, seed extensions and completion job; integration-test factories, base class and project files; `compose.yaml`, appsettings, README and solution references. Review transitive dependency overrides for Testcontainers.MsSql and retain any still-needed security fixes.

Remove the PostgreSQL deployment service; retain API or Seq containers where used. Docker remains required for SQL Server integration tests, but the application has no database-container dependency.

## 3. Target architecture and connection configuration

```text
HTTP controllers / Quartz job
    -> BLL managers
    -> Bll.Common persistence contracts
    -> SQL Server DAL repositories
    -> Microsoft.Data.SqlClient
    -> [TymchenkoOV].[BookingApp.*] stored procedures on remote SQL Server
    -> application tables
```

Expose `AddSqlServerDataAccess(connectionString, commandTimeoutSeconds)` from DAL for composition-root registration. BLL and controllers must not instantiate SqlConnection, use SqlException or execute SQL.

Implemented DAL layout:

```text
Infrastructure/   connection factory and typed procedure parameters
Repositories/     hall, booking, user repositories
Entities/         DAL persistence representations
Mappings/         SqlDataReader-to-entity hydration and entity/model AutoMapper profiles
Initialization/   explicit script runner and data seeders
    Scripts/      schema, procedures and permission scripts
```

Register a stateless connection factory and scoped repositories. Create a SqlConnection per operation, or one connection per explicit transaction; use SqlClient pooling instead of retaining a singleton open connection. Dispose connection, transaction, command and reader reliably. Materialize a page before returning it and do not share a connection across concurrent tasks. Keep Multiple Active Result Sets disabled unless a verified requirement justifies it.

Configuration retains the logical `ConnectionStrings:Database` key. A SQL-authentication example with placeholders is:

```text
Server=tcp:<host>,<port>;Database=<database>;User ID=<runtime-user>;Password=<secret>;Encrypt=True;TrustServerCertificate=False;Connect Timeout=15;Application Name=BookingApplication;
```

Use the authentication mode actually supported by the host: SQL credentials, integrated identity or supported Microsoft Entra authentication. Validate driver/runtime/server compatibility before selecting a package version. Store credentials in user secrets, environment variables or the deployment secret store, not committed appsettings, Compose files or documentation. Supply `ConnectionStrings__Database` to the app environment. Keep deployment and test credentials separate from runtime configuration. Never log complete connection strings or credentials.

Before implementation, validate remote connectivity, TLS certificate/hostname, firewall allowlisting/VPN and database access from developer, CI and deployment hosts. Set connect and command timeouts explicitly. An application readiness probe should verify database/procedure access without requiring DDL privileges; startup must not silently create or reseed a remote database.

## 4. Application contracts and transaction ownership

Change synchronous `void Add/Update/Remove` methods to asynchronous writes with cancellation and explicit provider-independent outcomes. The completed task means the database write has completed, rather than an EF change being tracked.

The implemented booking creation contract is:

```csharp
Task<ReservationOutcome> CreateReservationAsync(
    Booking booking,
    CancellationToken cancellationToken = default);
```

Outcomes in Bll.Common are Created, Overlap, HallNotFound and UserNotFound. Hall price revisions are not checked; the calculated price snapshot is preserved. Do not expose SqlException, SQL error numbers or provider row counts. Hall writes distinguish not found and referenced-by-bookings. User creation explicitly covers intended initial role assignments in its atomic operation.

Remove IUnitOfWork when all writes have atomic operation contracts. If a verified use case needs multiple procedure calls in one transaction, introduce an explicit application transaction abstraction backed by a DAL-owned SqlTransaction. Do not retain SaveChangesAsync as a no-op or reinterpret its integer as a procedure affected-row count.

Recommended ownership for the current operations: write procedures own their transactions and are invoked without an ambient application transaction. Each atomic procedure uses `SET NOCOUNT ON`, `SET XACT_ABORT ON`, and a TRY/CATCH with BEGIN TRANSACTION, COMMIT, rollback when `XACT_STATE() <> 0`, and THROW for unexpected failures. Domain-conflict paths must release/rollback their transaction before returning an outcome. Reads do not begin transactions unless snapshot consistency is an explicit requirement. Document ownership for every procedure.

If a future operation uses a caller-owned transaction, create/document a compatible procedure pattern. Do not blindly nest transaction-owning procedures: SQL Server nested transaction counters do not provide independently committable transactions.

Booking flow:

1. BLL validates, builds a UTC period, loads the hall and validates future start.
2. BLL calculates price and constructs the booking. A preliminary overlap read is optional and advisory.
3. DAL calls one reservation procedure that serializes competing writes, checks occupancy, inserts the booking and updates hall booking time atomically.
4. DAL maps the procedure outcome; BLL dispatches BookingCreatedDomainEvent only on successful commit.

Preserve the existing post-commit event-delivery limit: a handler can fail after data is committed. An outbox is separate scope if reliable delivery is required. Hall prices can change between read and reservation; preserve the current behavior explicitly or add a hall revision check under the reservation transaction. Narrow BookingManager's broad InvalidOperationException catch so database failures cannot be reported as invalid booking periods.

## 5. Stored procedure catalog and ADO.NET execution

Use a dedicated `TymchenkoOV` schema, explicit names and documented parameter/result contracts. Use SqlCommand with `CommandType.StoredProcedure`, schema-qualified names and typed SqlParameters. Avoid AddWithValue; set SqlDbType, string size, decimal precision/scale and DBNull explicitly.

| Current operation | Procedure | Result/behavior |
|---|---|---|
| Hall by ID | `hall_get` | Zero/one explicit hall row. |
| Hall pagination | `hall_list` | Validated page/size; ORDER BY ID with OFFSET/FETCH. |
| Availability | `hall_available` | UTC period/capacity; agreed occupancy rule; name then ID ordering. |
| Hall create/update/delete | `hall_create`, `hall_update`, `hall_delete` | Explicit outcomes; preserve restrictive deletion for referenced halls. |
| Booking by ID/page | `booking_get`, `booking_list` | Stable documented result schema and ID ordering. |
| Overlap precheck | `booking_has_overlap` | Scalar result or BIT output using the agreed policy. |
| Reservation creation | `booking_reserve` | Atomic booking insert plus hall timestamp update; explicit outcome. |
| Reporting enumeration | `booking_list` | ListAsync fetches materialized pages; no separate scan procedure or snapshot transaction. |
| Due reservations | `booking_due` | Reserved and end <= cutoff; ordered by end then ID; bounded size. |
| Completion batch | `booking_complete_due` | Atomic bounded status/timestamp update; explicit completed count. |
| User by ID | `user_get` | User, roles and role-permissions in three result sets loaded through NextResultAsync. |
| User creation | `user_create` | User and intended initial role assignments in one transaction. |

Specify result columns explicitly, never SELECT *. Use ExecuteReaderAsync for rows and explicit output parameters or documented result sets for outcomes. Do not infer success from ExecuteNonQueryAsync row counts, particularly with NOCOUNT. Output parameters are read after any reader is closed. Keep result-set ordering stable and distinguish absent rows from failures.

Version incompatible signatures/result contracts or coordinate their deployment. Use CREATE OR ALTER PROCEDURE only if supported by the actual server version. The migration runner must handle SQL Server batch boundaries; `GO` is a client separator, not executable T-SQL sent directly through SqlCommand. See [SQL Server CREATE PROCEDURE](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-procedure-transact-sql?view=sql-server-ver17).

## 6. SQL Server schema and concurrency

Preserve logical IDs, relationships, precision and domain semantics; translate physical provider-specific types and syntax.

| PostgreSQL source | SQL Server target | Mapping/validation |
|---|---|---|
| uuid | uniqueidentifier | Guid; preserve existing IDs. |
| numeric(18,2) | decimal(18,2) | Explicit parameter precision/scale. |
| timestamp with time zone | datetime2(7), stored as UTC | UTC-only contract; restore DateTimeKind.Utc on reading because datetime2 does not retain it. |
| varchar/text | nvarchar(length)/nvarchar(max) where justified | Size bounded fields; preserve Unicode; avoid unbounded columns for indexed names/email. |
| integer catalog IDs | int without automatic identity unless required | Preserve role/permission seed IDs. |
| PostgreSQL quoted identifiers | Explicit schema and bracketed identifiers | Preserve/standardize names deliberately; use `[start]`, `[end]`, `[Id]` if retained. |
| status strings and amenity text | Bounded status string and compatible amenity text | Preserve enum names and CSV numeric representation initially. |

Inspect source constraints and actual nullability. Define all primary/foreign keys, indexes, uniqueness, defaults and checks in T-SQL. Preserve no-cascade-delete semantics for halls/users referenced by bookings. Review SQL Server multiple-cascade-path limitations in role/user join tables. Choose collation deliberately: SQL Server case/accent comparison and unique email/name behavior may differ from PostgreSQL. Detect collisions before transfer. SQL Server uniqueidentifier ordering can differ from PostgreSQL UUID ordering; preserve stable API pagination, but do not promise the exact historical sequence without an explicit compatible ordering strategy.

Reconstruct Name, Capacity, Money, Currency, DateRange and user value objects explicitly. Preserve optional timestamps. Specify role hydration instead of relying on EF navigation loading. Keep BLL/DTO AutoMapper mappings and DAL entity/model AutoMapper profiles; readers hydrate DAL entities before mapping to business models.

### Concurrent booking protection

Implemented policy: only Reserved blocks occupancy in both availability and booking checks. Intervals remain half-open: existing.start < requested.end AND existing.end > requested.start; adjacent bookings are valid.

SQL Server has no direct equivalent to PostgreSQL range exclusion constraints. Use transaction-owned exclusive `sys.sp_getapplock` with a canonical hall resource name (for example `BookingHall:<canonical-guid>`), acquired inside the reservation transaction. Set a bounded lock timeout, inspect every return code and explicitly roll back on failure. Under the lock, check the hall/user and overlapping reservations, then insert and update hall timestamp. Transaction-owned locks release at transaction end. See [sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver17).

Every procedure that changes hall/time into a blocking reservation must acquire the same lock; moving a reservation between halls must acquire both hall locks in a deterministic order. Privileged import/admin writes must follow the same rule or execute during exclusive maintenance. EXECUTE-only runtime permissions prevent ordinary callers bypassing this protocol. A tested UPDLOCK/HOLDLOCK strategy locking the hall row is an alternative, but choose one coherent approach rather than mixing protocols.

Add CHECK(start < end) and evaluate an occupancy index on hall/status/start including end. Evaluate a filtered due index on end/ID WHERE status = 'Reserved'. Use actual execution plans and STATISTICS IO/TIME with representative data; stored procedures do not guarantee performance improvements.

For completion, use a bounded ordered selection and guarded UPDATE in one transaction; capture actual affected count before other statements modify @@ROWCOUNT. The implemented completion procedure uses UPDLOCK and guarded updates to serialize competing workers. READPAST is not used; verify the remote isolation/RCSI configuration separately. Capture one UTC cutoff per run and commit each batch separately. Repeated runs must not double-count or overwrite later statuses.

Hall updates must only write editable fields, avoiding stale last_booked_on_utc. Preserve last-write-wins explicitly, or add rowversion and a conflict outcome as a deliberate enhancement. A rowversion is a concurrency token, not a timestamp.

## 7. Permissions, error translation and remote operations

Use separate deployment and runtime identities, and a dedicated runtime database role. Grant EXECUTE on specific approved procedures or a tightly controlled procedure-only schema. Avoid db_owner, db_datareader and db_datawriter for runtime. Inspect inherited/public permissions.

Use same-owner static SQL ownership chaining for procedure access to tables. Where it is insufficient, use narrowly scoped module signing or documented execution context reviewed for the remote environment. Dynamic SQL does not receive ordinary ownership-chain protection; avoid it or handle permissions explicitly. Do not blindly DENY table access without testing any signed/execution-context design. Verify with runtime credentials that approved procedures succeed and direct SELECT/INSERT/UPDATE/DELETE and DDL fail.

The implementation maps explicit procedure outcomes for known business conflicts. Unexpected SQL failures are logged in DAL and translated by SQL number into provider-independent `PersistenceException` categories, without retaining raw provider exceptions. It does not retry writes. The following table remains guidance for operation-specific business translation or future retry policies:

| Failure | Handling |
|---|---|
| Explicit overlap outcome | Existing booking overlap result. |
| Explicit missing/referenced-row outcome | Operation-specific not-found/conflict result. |
| 547 FK/check violation | Translate only when the operation/contract identifies the expected failure; otherwise internal failure. |
| 2601 / 2627 duplicate | Operation-specific duplicate outcome; do not conflate all keys. |
| 1205 deadlock victim | Bounded full-operation retry only when safe. |
| Command timeout, lock timeout, connection/auth/TLS failure | Operational diagnostics; distinguish from domain errors. |
| Cancellation | Dispose/rollback; preserve caller cancellation. |
| Unknown error | Sanitized API error plus internal diagnostics. |

Do not automatically replay writes after an uncertain commit/network loss. Reconcile by the stable booking ID or introduce deliberate idempotency. Include remote latency, blocked sessions, pool exhaustion and procedure duration in monitoring without sensitive parameter values. Define who owns backup/restore, schema deployment and remote availability.

## 8. Schema deployment and optional cross-engine data transfer

For initial setup, create SQL Server schema using the user's code-first approach, add the stored procedures and required data/permissions, then generate a reusable setup script from that database. Verify its schema matches the DAL contract and rehearse it on an empty database. Include required seed data explicitly when schema-only scripting omits it. No repository deployment-tool project is included; schema setup requires appropriate administrative privileges. When DatabaseSeeding:Enabled is true, startup invokes IDatabaseSeeder.InitializeAsync to apply unapplied scripts before reference/demo data methods. The main database connection is used unless an optional Seeding connection overrides it. Integration fixtures use the same runner and data seeders.

Installation paths:

1. Empty approved SQL Server database: apply the new T-SQL baseline, procedures, reference seeds and grants.
2. Existing SQL Server database: inventory objects/data and verify compatibility before adoption or incremental changes. Never overwrite unrelated objects or infer a baseline from a PostgreSQL EF history record.
3. Existing PostgreSQL data to preserve: create SQL Server schema, then execute a one-time validated export/import. PostgreSQL migrations cannot be run on SQL Server or marked as an equivalent SQL Server baseline without conversion.

Transfer steps when required:

- Back up/export the source and rehearse on non-production target.
- Detect invalid intervals, existing overlaps, collation/unique collisions, unknown enum/currency values, numeric overflow and orphaned relationships.
- Export exact GUID/catalog IDs, decimal values and timestamps normalized to UTC.
- Load parent tables (halls, users, roles, permissions), then bookings and joins with constraints enabled; use staging if validation requires it.
- Use one-time tooling or SqlBulkCopy with controlled deployment permissions. Temporary source-reading dependencies stay outside runtime and are removed after transfer.
- Compare per-table counts and key sets, referential integrity, price totals, timestamp samples and role assignments; verify sample API behavior.
- Freeze old writes for final export/load and drain the completion job. Without a designed incremental replication path, plan a maintenance window rather than running both writable systems.
- Record transfer completion and retain source backup/export under an agreed retention policy; do not destroy the source as part of routine cutover.

The reference seeder includes the mandatory role/permission catalog and temporary API user. Demo halls are a separate optional seeder method. Replace the temporary user when authentication is implemented. Seeds are idempotent and target-checked. Demo seeds are explicit development/test actions, never automatic against a production remote connection.

## 9. SQL Server integration-test containers

Replace Testcontainers.PostgreSql and PostgreSqlBuilder with `Testcontainers.MsSql`, `MsSqlBuilder` and `MsSqlContainer`. Keep asynchronous fixture startup/disposal. Testcontainers manages ephemeral test containers; no SQL Server database service is added to deployment Compose. See the [Testcontainers for .NET MSSQL module](https://dotnet.testcontainers.org/modules/mssql/).

Pin a supported SQL Server image tag/version, preferably digest, matching the remote engine major version where available. Match compatibility level, collation and isolation/RCSI settings. Verify developer/CI Docker architecture, image support, memory and SQL Server license/EULA requirements. Containers do not fully reproduce remote networking, authentication or hosting restrictions; verify those separately during deployment. An Azure SQL target may have additional differences from a SQL Server container.

Fixture lifecycle:

1. Start the container with ephemeral credentials and a bounded readiness timeout.
2. Create a test database and apply the same T-SQL migrations, procedures, reference seeds and grants used for remote deployment; do not maintain a simplified test schema.
3. Use the administrator connection only for setup/assertion helpers. Create a restricted runtime test login/user with EXECUTE-only permissions.
4. Configure both WebApplicationFactory implementations with the runtime container connection before application startup and register the production SqlClient DAL. Never fall back to the remote application connection.
5. Dispose application hosts/connections and then the container, including failure paths. Capture useful diagnostics without credentials.

Replace BaseIntegrationTest.DbContext and EF assertions with SQL fixture helpers or public reads. Administrative SQL stays in tests/deployment tooling. Generate test credentials during setup rather than reusing remote credentials. Container-only certificate trust settings stay in test configuration; remote certificate validation remains enabled.

Prefer one container per test collection/run with a separate database per independently executing suite. Shared databases require a nonparallel test collection and deterministic fixture reset. Use unique IDs and targeted cleanup where practical, and verify resets target the fixture-created container database. A per-test client transaction cannot isolate HTTP requests or jobs using independent connections. Share bootstrap utilities between integration projects without allowing competing migrations/resets on the same database.

Disable scheduled completion in ordinary API fixtures and test it explicitly in job tests. Concurrency tests use independent connections to the same database and synchronized writes. Permission tests execute through the runtime identity, not the administrator. CI must provide a supported Docker engine and sufficient resources; integration jobs fail clearly on startup/migration errors rather than silently skip. Unit tests remain database/Docker independent. Optional remote staging smoke tests use separately supplied credentials and perform no destructive resets.

Required coverage: mapping/Unicode/UTC/nulls; CRUD and deterministic pagination; each blocking status; overlap/adjacent periods; reservation rollback; two synchronized competing connections; completion reruns and competing workers; runtime permissions; timeout/cancellation disposal; migrations rerun/checksum/concurrency; empty installation and optional data transfer; existing HTTP validation, serialization and event behavior.

## 10. Implementation phases and gates

| Phase | Work | Acceptance gate |
|---|---|---|
| 0 — baseline and remote prerequisites | Inventory behavior; obtain remote deployment details; verify Docker/test image support; decide data transfer and occupancy policy. | Reviewed contracts, remote prerequisites and working SQL Server test container. |
| 1 — SQL Server setup | Code-first schema, procedures/reference data/roles, generated initial setup script; rehearse data conversion if required. | Generated setup works on an empty database; container scripts and remote schema agree. |
| 2 — ADO.NET reads | Rename DAL project/references; SqlClient dependency/factory; stored read procedures; explicit mapping and diagnostics. | Read contracts, Unicode/UTC/decimal/null and ordering tests pass. |
| 3 — asynchronous writes | Application outcomes, manager updates, write procedures, transaction ownership, hall application locks and precise errors. | Atomicity and concurrent overlap tests pass; API contracts retained. |
| 4 — job/bootstrap/tests | Completion procedure, test script bootstrap, SQL Server container factories/assertions; remove PostgreSQL fixtures and startup EF paths. | BLL/API/job tests pass in SQL Server containers using runtime permissions; application uses remote SQL Server. |
| 5 — cutover and cleanup | Data freeze/load if needed; switch remote connection; drain old instances; remove EF/Npgsql/PG artifacts and update docs/config. | No runtime PostgreSQL/EF dependency; Testcontainers.MsSql is test-only; tested deployment and rollback runbook. |

Remove EF Core/provider/tools references from DAL, web and tests once no caller needs them. Remove context, generic EF repository and EF formatters. Retain provider-independent DAL persistence entities and entity/model AutoMapper profiles. The old PostgreSQL EF migrations are removed from the source tree; Git history retains the baseline. Inspect unrelated API-versioning/Quartz dependencies in DAL and remove only unused ones.

Remove the Compose PostgreSQL service, database depends_on/health checks, volume and hardcoded credentials; configure the API for remote SQL Server. Update appsettings examples, README, English/Ukrainian DAL/testing docs and CI scripts. SQL Server containers exist only in integration-test fixtures; document their Docker prerequisites. Do not install LocalDB or add a database service to application deployment.

## 11. Cutover, rollback and estimate

Deploy compatible SQL Server schema/procedures first, validate permissions and remote reachability, then drain existing writers/jobs. Transfer data if required and switch the whole application DAL/configuration together. Never split hall updates and booking inserts across PostgreSQL and SQL Server. Verify API smoke tests and completion/report behavior before accepting writes.

Once new SQL Server writes are accepted, switching back to the old PostgreSQL database loses those writes unless a reverse-transfer/reconciliation plan exists. Prefer rollback to a previous tested application version that already supports the same SQL Server schema. Before SQL Server writes begin, aborting cutover can restore the old deployment as a temporary recovery measure; PostgreSQL remains excluded from the final architecture. Do not treat a provider toggle as a sufficient cross-database rollback strategy.

Rehearse backup/restore or roll-forward with the remote database owner, define the maintenance window and recovery responsibility, and keep schema changes additive through the rollback window. Do not drop remote databases or source data during ordinary rollback.

Planning estimate: approximately 12–20 engineering days for one developer, plus review, remote provisioning and deployment coordination. Optional production data transfer and remote deployment restrictions can expand it. Refine after Phase 0; this is not a commitment.

## 12. Definition of done

- Runtime uses Microsoft.Data.SqlClient and remote SQL Server stored procedures for all persistence.
- PostgreSQL/Npgsql and PostgreSQL containers are removed; Testcontainers.MsSql remains only in integration-test projects.
- Remote runtime/deployment credentials and ephemeral container test credentials are separated; secrets are externalized.
- BLL/controllers expose no SQL Server/provider types; no fake unit of work remains.
- Reservations are atomic and concurrency-safe; agreed occupancy semantics are consistent.
- Seeding, deployment, user roles, reporting and background completion operate without EF.
- SQL Server container integration tests and EXECUTE-only permission checks pass; remote deployment connectivity is separately verified.
- Existing data is transferred and reconciled if required; cutover/rollback is rehearsed.
- Public API compatibility and documented intentional behavior changes are verified.

References: [Microsoft.Data.SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace?view=sql-server-ver17), [SQL Server stored procedures](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-procedure-transact-sql?view=sql-server-ver17), [SQL Server application locks](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver17).
