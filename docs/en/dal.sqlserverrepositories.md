# SQL Server DAL

`BookingApp.Dal.SqlServerRepositories` implements Bll.Common repositories using Microsoft.Data.SqlClient and schema-qualified stored procedures. BLL knows no SQL Server types. Connections are pooled and opened per operation; commands/readers are disposed asynchronously. Parameters have explicit SQL types, sizes, precision and scale.

Readers hydrate DAL persistence entities and restore UTC timestamp kinds. The DAL AutoMapper profile converts entities to business models/value objects and maps writes back to entities. User roles/permissions are loaded explicitly. Entities and mappings stay in DAL; EF context and entity tracking are removed. Async writes persist immediately. The reservation procedure commits the insert and hall timestamp together; IUnitOfWork is removed rather than simulated.

Only Reserved bookings block occupancy. The precheck is advisory; the atomic write enforces overlap under a transaction-owned per-hall application lock. Adjacent periods are valid. Hall edits preserve last-booked timestamps and deleting a referenced hall returns a controlled conflict. Completion uses guarded bounded updates and reports actual counts.

Use remote SQL Server for the application. Build the initial schema with the external code-first setup, install procedures/reference data and generate the reusable setup script from the database. No EF context or code-first bootstrap project is included here. Consult the [versioned SQL scripts](../../BookingApp.Dal.SqlServerRepositories/Database/README.md) explicitly with separate deployment credentials. The runtime role has EXECUTE only; startup does not create databases or seed data. SQL Server containers exist only in integration tests and run the same scripts using the production DAL.

Quartz, TimeProvider and event handlers remain in Services. Pricing/validation remain in BLL. See [Testing](testing.md) and the [migration plan](ef-to-ado-net-migration-plan.md).

## Entities and mapping boundary

`Entity` supplies a `Guid Id` to `BookingEntity`, `ConferenceHallEntity` and `UserEntity`. `RoleEntity` and `PermissionEntity` have integer catalog keys. `UserRoleEntity` and `RolePermissionEntity` use their two foreign keys as a composite identity; they have no artificial Guid key.

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

All procedure names are qualified with `booking_api`. Pages are one-based and page/batch sizes must be positive. `ListAsync` opens a separate connection per materialized page; it does not promise a consistent snapshot across concurrent writes. UTC input is required, datetime2 parameters use scale 7, and money parameters use decimal(18,2). Business outcomes are explicit integer output parameters; unknown values fail rather than silently becoming success. Unexpected SQL failures propagate to centralized API error handling. No automatic retries or provider exception-to-business-error translation are implemented.

## Registration and configuration

The Services composition root registers the DAL AutoMapper profile together with BLL/HTTP profiles, then calls `AddSqlServerDataAccess(connectionString, commandTimeoutSeconds)`. The factory is singleton and holds configuration only; repositories are scoped. Each operation opens and disposes its own pooled connection. Write procedures own the atomic transaction; callers do not wrap multi-statement procedures in an ambient transaction.

| Setting | Purpose / default |
| --- | --- |
| `ConnectionStrings:Database` | Required remote runtime connection string; use `ConnectionStrings__Database` or user secrets. |
| `Database:CommandTimeoutSeconds` | Positive command timeout; defaults to 30 seconds. Connection timeout is controlled by the connection string. |
| `BackgroundJobs:CompleteBookings:Enabled` | Scheduler registration switch; defaults to true. |
| `BackgroundJobs:CompleteBookings:IntervalSeconds` / `PageSize` | Completion schedule and bounded write size, configured in web appsettings. |

Registration validates the connection string and command timeout but does not open a connection, verify remote access or apply SQL. Provision and validate the remote schema, permissions and connectivity separately. Integration fixtures use the local embedded SQL artifacts and an EXECUTE-only identity; they never use the remote application connection string.
