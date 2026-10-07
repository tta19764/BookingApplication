# SQL Server initialization and data seeding

The EF/PostgreSQL DAL has been replaced by ADO.NET (`Microsoft.Data.SqlClient`) repositories calling `booking_api` stored procedures. The application connects to a configured SQL Server database. SQL Server containers are used only by integration tests. See [refactoring](refactoring.md) for the architectural changes and [DAL contracts](dal.sqlserverrepositories.md) for procedure and mapping behavior.

## Responsibilities and execution order

The Common `IDatabaseSeeder` interface is implemented by DAL `Initialization/DatabaseSeeder` and registered as scoped in Services. `StartupDataSeeder` creates an async scope and awaits the following sequence before HTTP requests or background jobs start:

1. `InitializeAsync`: apply or verify embedded schema, procedure and permission scripts.
2. `SeedReferenceDataAsync`: add missing required reference rows.
3. `SeedDemoDataAsync`: add sample halls only when enabled and the hall catalog is empty.

`InspectAsync` reports row counts for the seven application tables and `HasData`; it does not inspect or repair schema metadata. Initialize the schema before invoking inspection or data methods. There is no separate `DatabaseSchemaInspector`.

## Configuration

```json
{
  "ConnectionStrings": {
    "Database": "Server=<server>;Database=BookingApplication;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  },
  "DatabaseSeeding": {
    "Enabled": true,
    "IncludeDemoData": false
  }
}
```

This example uses Windows authentication and a trusted local development certificate. Use the authentication and certificate settings required by your deployment.

| Setting | Behavior |
| --- | --- |
| `ConnectionStrings:Database` | Required application connection. Also used for initialization/seeding unless overridden. |
| `ConnectionStrings:Seeding` | Optional setup connection; missing, empty or whitespace falls back to `Database`. |
| `DatabaseSeeding:Enabled` | Enables scripts and reference seeding; base configuration defaults to false. |
| `DatabaseSeeding:IncludeDemoData` | Adds demo halls after reference seeding; base configuration defaults to false and this flag has no effect when `Enabled=false`. |

`appsettings.Development.json` is local-only and ignored by Git; configure its flags explicitly. The tracked base appsettings disables both flags. Environment variables override appsettings; use double underscores, for example `DatabaseSeeding__Enabled`. Compose explicitly defaults both flags to false and requires `ConnectionStrings__Database` from the shell or Compose `.env`; it does not read appsettings to interpolate `${...}`. Set the Compose flags explicitly when container startup should initialize data.

A single connection string is sufficient when its identity has setup permissions. If the application uses an EXECUTE-only runtime identity, supply a separate `Seeding` identity. New scripts require table/procedure/role creation and permission-grant rights; data methods require SELECT/INSERT access. The permission script defines a runtime role but does not provision a login or assign an application user to it. Keep credentials outside committed configuration.

## Scripts and repeat runs

The DAL embeds these scripts from `Initialization/Scripts`:

- [001_schema.sql](../../BookingApp.Dal.SqlServerRepositories/Initialization/Scripts/001_schema.sql): seven application tables, keys, checks and indexes.
- [002_stored_procedures.sql](../../BookingApp.Dal.SqlServerRepositories/Initialization/Scripts/002_stored_procedures.sql): `booking_api` procedure contracts.
- [003_permissions.sql](../../BookingApp.Dal.SqlServerRepositories/Initialization/Scripts/003_permissions.sql): `booking_runtime` role and EXECUTE grant; no application data.

`DatabaseInitializer.ApplyAsync` acquires an initialization lock and applies scripts in filename order. Each script and its journal entry commit together. `dbo.booking_schema_versions` stores the version, normalized SHA256 checksum and application time. Unchanged applied scripts are skipped; editing an applied script causes failure. Add a new versioned script for changes. The runner supports standalone `GO` separators, not arbitrary sqlcmd directives.

The target database must already exist; system databases are rejected. Existing unjournaled application tables are rejected by the baseline. An external code-first/manual schema needs a reviewed compatible baseline before enabling the runner; do not silently mark scripts as applied.

`Database/` is personal development material, ignored by Git and excluded from compilation/resources/content. Generated personal setup scripts can be kept there; the application never reads them.

## Data behavior

Reference seeding fills missing roles, permissions, relationship links and the temporary API user. It preserves existing data and fails on conflicting reserved identities rather than overwriting them. Unrelated existing application data does not prevent missing reference rows from being added.

Demo seeding inserts Hall A/B/C only when `conference_halls` is empty. Any existing hall skips the entire demo operation, including a partially populated demo catalog.

Both data operations acquire the same transaction-owned application lock and use serializable transactions. `SeedResult` returns previous row counts, committed inserted-row count and the demo skip flag. Failures or cancellation roll back the current operation. Initialization scripts and each data operation commit separately; a later failure does not undo earlier completed steps. Correct the failure and restart to continue through the repeat-safe flow. Startup failures propagate and prevent the host from accepting requests.

## Explicit runtime invocation

Resolve the registered interface from a DI scope when initialization/seeding is needed outside startup:

```csharp
await using var scope = services.CreateAsyncScope();
var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
await seeder.InitializeAsync(cancellationToken);
var before = await seeder.InspectAsync(cancellationToken);
var reference = await seeder.SeedReferenceDataAsync(cancellationToken);
var demo = await seeder.SeedDemoDataAsync(cancellationToken); // Optional.
```

The configuration flags control the startup coordinator; they do not prohibit explicit interface calls. Data methods do not independently run scripts. No public HTTP seeding endpoint is provided.

## Troubleshooting and tests

`Invalid object name 'dbo.conference_halls'` means the selected database lacks the expected table. Check the server/database in the selected connection and call `InitializeAsync` before inspection or data methods. Enabled startup already performs that ordering. If existing tables are unjournaled, resolve the baseline instead of replaying the initial schema. A checksum mismatch requires restoring the original applied script and adding a new version for the change.

Integration fixtures apply the same embedded scripts and invoke the data methods against disposable SQL Server databases, then exercise repositories with restricted runtime credentials. Startup coordinator tests verify ordering, disabled behavior, connection fallback and failure propagation. These tests do not use the application's remote connection string. See [testing](testing.md).

SQL errors from initialization, inspection and data seeding become sanitized Common `PersistenceException` values and are logged by DAL when a logger is configured. Startup still fails; the exception carries an incident ID for correlation. Raw SqlClient exceptions are not exposed through the Common interface.
