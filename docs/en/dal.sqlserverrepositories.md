# SQL Server DAL

`BookingApp.Dal.SqlServerRepositories` implements Bll.Common repositories using Microsoft.Data.SqlClient and schema-qualified stored procedures. BLL knows no SQL Server types. Connections are pooled and opened per operation; commands/readers are disposed asynchronously. Parameters have explicit SQL types, sizes, precision and scale.

Readers hydrate DAL persistence entities and restore UTC timestamp kinds. The DAL AutoMapper profile converts entities to business models/value objects and maps writes back to entities. User roles/permissions are loaded explicitly. Entities and mappings stay in DAL; EF context and entity tracking are removed. Async writes persist immediately. The reservation procedure commits the insert and hall timestamp together; IUnitOfWork is removed rather than simulated.

Only Reserved bookings block occupancy. The precheck is advisory; the atomic write enforces overlap under a transaction-owned per-hall application lock. Adjacent periods are valid. Hall edits preserve last-booked timestamps and deleting a referenced hall returns a controlled conflict. Completion uses guarded bounded updates and reports actual counts.

Use remote SQL Server for the application. Build the initial schema with the external code-first setup, install procedures/reference data and generate the reusable setup script from the database. No EF context or code-first bootstrap project is included here. Consult the [versioned SQL scripts](database-initialization.md) explicitly with separate deployment credentials. The runtime role has EXECUTE only; startup never creates the database itself; when enabled it applies schema/procedure/permission scripts before seeding data. SQL Server containers exist only in integration tests and run the same scripts using the production DAL.

Quartz, TimeProvider and event handlers remain in Services. Pricing/validation remain in BLL. See [Testing](testing.md) and the [migration plan](ef-to-ado-net-migration-plan.md).

## Entities and mapping boundary

`Entity<TKey>` supplies a typed primary key to single-key persistence entities. `BookingEntity`, `ConferenceHallEntity` and `UserEntity` inherit `Entity<Guid>`; `RoleEntity` and `PermissionEntity` inherit `Entity<int>` for their catalog keys. `UserRoleEntity` and `RolePermissionEntity` use their two foreign keys as a composite identity; they have no artificial Guid key.

Storage values are primitives: decimal price amounts, three-letter currency codes, status names and comma-separated numeric amenity values. A currency lookup table has not been added. Nullable lifecycle timestamps remain nullable. `RowMapper` uses named result columns to hydrate entities; the DAL `AutoMapperConfig` reconstructs Common value objects and maps writes back to storage values. The repository assembles relationships; AutoMapper does not fetch data. Hall and booking reads do not load related bookings, halls or users.

`user_get` returns three result sets: the user row, role rows, then permission rows with a `role_id`. `RowMapper.User`, `Role` and `Permission` hydrate each row; `UserRepository` attaches permissions to roles before mapping the complete user. User creation uses distinct existing role IDs and rolls back the user insertion if any role is invalid.

## Operation contracts

| Repository operation | Stored procedure | Result |
| --- | --- | --- |
| Hall get / list / availability | `hall_get`, `hall_list`, `hall_available` | Optional hall, materialized page, or capacity-filtered available halls. |
| Hall create / update | `hall_create`, `hall_update` | Immediate creation; update returns false when absent and preserves last-booked time. |
| Hall removal | `hall_delete` | `Removed=0`, `NotFound=1`, `HasBookings=2`; referenced hall deletion maps to HTTP 409. |
| Booking get / list / report enumeration | `booking_get`, `booking_list` | Optional booking or materialized pages ordered by ID. |
| Overlap / reserve | `booking_has_overlap`, `booking_reserve` | Advisory overlap check; atomic write returns `Created=0`, `Overlap=1`, `HallNotFound=2`, `UserNotFound=3`. |
| Due read / complete | `booking_due`, `booking_complete_due` | Bounded read without claiming rows; guarded status update returns the actual completed count. |
| User get / create | `user_get`, `user_create` | User with roles/permissions, or atomic user and role-link creation. |

All procedure names are qualified with `booking_api`. Pages are one-based and page/batch sizes must be positive. `ListAsync` opens a separate connection per materialized page; it does not promise a consistent snapshot across concurrent writes. UTC input is required, datetime2 parameters use scale 7, and money parameters use decimal(18,2). Business outcomes are explicit integer output parameters; unknown values fail rather than silently becoming success. SQL failures are logged and translated within DAL to Common `PersistenceException`, carrying a `PersistenceError` category, logical operation and incident ID. Raw provider exceptions are not attached as inner exceptions. API middleware returns a generic HTTP 500 with the incident ID; it does not duplicate DAL logging. Cancellation remains cancellation, and no automatic retries are implemented. Expected procedure outcomes remain business results.

## Registration and configuration

The Services composition root registers the DAL AutoMapper profile together with BLL/HTTP profiles, then calls `AddSqlServerDataAccess(connectionString, commandTimeoutSeconds)`. The factory is singleton and holds configuration only; repositories are scoped. Each operation opens and disposes its own pooled connection. Write procedures own the atomic transaction; callers do not wrap multi-statement procedures in an ambient transaction.

| Setting | Purpose / default |
| --- | --- |
| `ConnectionStrings:Database` | Required remote runtime connection string; use `ConnectionStrings__Database` or user secrets. |
| `Database:CommandTimeoutSeconds` | Positive command timeout; defaults to 30 seconds. Connection timeout is controlled by the connection string. |
| `BackgroundJobs:CompleteBookings:Enabled` | Scheduler registration switch; defaults to true. |
| `BackgroundJobs:CompleteBookings:IntervalSeconds` / `PageSize` | Completion schedule and bounded write size, configured in web appsettings. |

Registration validates the connection string and command timeout but does not open a connection, verify remote access or apply SQL. Provision and validate the remote schema, permissions and connectivity separately. Integration fixtures use the local embedded SQL artifacts and an EXECUTE-only identity; they never use the remote application connection string.

## Explicit database seeding

`Initialization/DatabaseSeeder` implements the Common `IDatabaseSeeder` interface; its result models also live in Common. InitializeAsync runs script initialization before data seeding at enabled startup. The Services composition root always registers it as scoped and registers `StartupDataSeeder` to run it before requests/background jobs when enabled. An optional seeding connection string can override the main connection. Otherwise the main connection is reused and must have SELECT/INSERT permissions for seeding.

```csharp
var seeder = new DatabaseSeeder(setupConnectionString);
await seeder.InitializeAsync(cancellationToken);
var inspection = await seeder.InspectAsync(cancellationToken);
var reference = await seeder.SeedReferenceDataAsync(cancellationToken);
// Optional; explicit invocation only for development/test setup.
var demo = await seeder.SeedDemoDataAsync(cancellationToken);
```

Apply schema and procedure scripts before invoking this service. The seeder has no metadata schema inspector. InitializeAsync applies required scripts using the journal; it needs schema/procedure/permission setup rights when scripts are unapplied. Data methods require SELECT/INSERT. Scripts own the schema contract. System databases are rejected.

`InspectAsync` reports `RowCounts` for each table and `HasData` when any application table contains rows. Each seed operation reads the existing data counts under a serializable transaction and a common transaction-owned application lock. `SeedResult` returns the before-state, committed inserted-row count and demo skip flag. Failed writes or cancellation roll back the operation.

Reference seeding adds only missing required rows, even when unrelated application data already exists. Reserved IDs/names and the temporary user email must agree with existing identities; conflicts fail without overwriting existing data. Demo seeding adds Hall A/B/C only when `conference_halls` is empty; any existing hall causes a complete skip, including a partial demo catalog. Existing reference data does not prevent demo seeding. The data methods install no procedures or runtime grants; `InitializeAsync` applies those scripts first. Include the required data before exporting a personal reusable setup script.

## Opt-in startup data seeding

```json
"DatabaseSeeding": {
  "Enabled": true,
  "IncludeDemoData": false
}
```

Both flags default to false. `Enabled=true` first initializes scripts, then runs reference seeding; `IncludeDemoData=true` additionally seeds demo halls after reference seeding succeeds. With Enabled=false neither operation runs, regardless of the demo flag, and the seeder is not resolved. Registration remains present through the Common interface.

Optionally provide `ConnectionStrings__Seeding` through environment variables or user secrets. When missing or blank it falls back to `ConnectionStrings__Database`. The selected identity needs SELECT/INSERT access for data and setup rights for unapplied schema/procedure/permission scripts; a separate identity permits keeping runtime EXECUTE-only. Conflicts, database failures or cancellation prevent startup rather than allowing requests with incomplete required data. The startup coordinator awaits the methods inside an async DI scope before `app.Run`; it logs committed counts without credentials. It first calls IDatabaseSeeder.InitializeAsync, which applies unapplied schema/procedure/permission scripts through DatabaseInitializer. API test fixtures explicitly disable startup seeding and seed their disposable database separately.

Enabled startup now executes scripts before data methods. The target database must already exist. Use credentials that can create tables/procedures/roles and grant permissions when initialization is needed. Existing unjournaled tables are not silently adopted: for external code-first/manual schemas, establish an explicit reviewed baseline before enabling the runner. Add versioned scripts for changes instead of editing applied ones.

DAL logs record SQL number/state/class and the incident ID, excluding SQL text, parameter values, connection strings and provider messages. Application DI supplies the logger factory; explicit setup callers can pass a logger factory to `DatabaseSeeder` or a logger to `DatabaseInitializer.ApplyAsync`. Without an explicit logger, direct construction still translates failures but emits no DAL diagnostic log.
