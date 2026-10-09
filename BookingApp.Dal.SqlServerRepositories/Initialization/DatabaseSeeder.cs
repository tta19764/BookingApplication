using Microsoft.Extensions.Logging;
using System.Data;
using BookingApp.Bll.Common.Initialization;
using BookingApp.Bll.Common.Initialization.Models;
using System.Collections.ObjectModel;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Initialization;

/// <summary>Explicitly seeds an existing SQL Server application database.</summary>
/// <remarks>Use setup credentials with table SELECT and INSERT access. Startup may resolve IDatabaseSeeder when configured. Its credentials require schema/procedure/permission setup access and table SELECT/INSERT access; an EXECUTE-only identity cannot seed data.</remarks>
public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private readonly SqlConnectionFactory _connections;
    private readonly string _connectionString;

    /// <summary>Creates a setup-only seeder without opening a connection.</summary>
    /// <param name="setupConnectionString">Connection string for the already-created application database with administrative setup permissions.</param>
    /// <param name="logger">Required seeder diagnostic logger, supplied by the caller or DI.</param>
    /// <param name="commandTimeoutSeconds">Positive execution timeout in seconds, defaulting to 30.</param>
    public DatabaseSeeder(string setupConnectionString, ILogger<DatabaseSeeder> logger, int commandTimeoutSeconds = SqlConnectionFactory.DefaultCommandTimeoutSeconds)
    {
        _connections = new SqlConnectionFactory(setupConnectionString, logger, commandTimeoutSeconds);
        _connectionString = setupConnectionString;
    }

    /// <summary>Applies unapplied schema, procedure, and permission scripts before data seeding.</summary>
    /// <param name="cancellationToken">Cancels script initialization.</param>
    /// <returns>A task completed when all embedded initialization scripts have been applied or verified.</returns>
    /// <remarks>Applied checksums prevent replay and detect changed scripts. An existing unjournaled schema is not automatically adopted.</remarks>
    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        DatabaseInitializer.ApplyAsync(_connectionString, cancellationToken, _connections.Logger,
            commandTimeoutSeconds: _connections.CommandTimeoutSeconds);

    /// <summary>Counts existing rows in the seven application tables without modifying data.</summary>
    /// <param name="cancellationToken">Cancels connection opening and counting.</param>
    /// <returns>Existing application data counts; no data or schema is modified.</returns>
    /// <exception cref="InvalidOperationException">The connection targets a system database.</exception>
    public async Task<DatabaseInspection> InspectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connections.OpenAsync(cancellationToken);
            return await ReadDataAsync(connection, null, cancellationToken);

        }
        catch (SqlException exception)
        {
            throw _connections.Translate(exception, "DatabaseSeeder.InspectAsync", cancellationToken);
        }
    }

    /// <summary>Atomically adds missing roles, permissions, links, and the temporary API user.</summary>
    /// <param name="cancellationToken">Cancels database operations, including lock acquisition.</param>
    /// <returns>The previous data counts and number of committed inserts; repeated execution adds no duplicates.</returns>
    /// <remarks>Existing values are preserved. Conflicting reserved IDs or names fail and roll back the entire operation. This method does not install procedures or grant permissions.</remarks>
    public Task<SeedResult> SeedReferenceDataAsync(CancellationToken cancellationToken = default) =>
        SeedAsync(false, cancellationToken);

    /// <summary>Atomically adds the three sample halls only when the hall catalog is empty.</summary>
    /// <param name="cancellationToken">Cancels database operations, including lock acquisition.</param>
    /// <returns>The previous data counts, insertion count and whether an existing catalog caused the operation to be skipped.</returns>
    /// <remarks>Other application tables may already contain data. Existing halls are never overwritten or supplemented with demo rows.</remarks>
    public Task<SeedResult> SeedDemoDataAsync(CancellationToken cancellationToken = default) =>
        SeedAsync(true, cancellationToken);

    private async Task<DatabaseInspection> ReadDataAsync(SqlConnection connection, SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (new[] { "master", "model", "msdb", "tempdb" }.Contains(connection.Database, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Specify a dedicated application database for seeding.");

        var counts = new Dictionary<string, long>();
        // Table names are fixed application values. Scripts own schema setup; SQL errors propagate if it is missing.
        foreach (var (table, objectName) in new[]
        {
            ("conference_halls", "ConferenceHalls"), ("users", "Users"), ("roles", "Roles"),
            ("permissions", "Permissions"), ("role_permissions", "RolePermissions"),
            ("user_roles", "UserRoles"), ("bookings", "Bookings")
        })
        {
            await using var command = new SqlCommand($"SELECT COUNT_BIG(*) FROM [TymchenkoOV].[BookingApp.{objectName}]", connection, transaction);
            command.CommandTimeout = _connections.CommandTimeoutSeconds;
            counts[table] = (long)(await command.ExecuteScalarAsync(cancellationToken))!;
        }
        return new DatabaseInspection(new ReadOnlyDictionary<string, long>(counts));
    }

    private async Task<SeedResult> SeedAsync(bool demo, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _connections.OpenAsync(cancellationToken);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            // Both seed operations use the same database-local lock; it is released by commit or rollback.
            await using var acquire = new SqlCommand("""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource=N'BookingApplication.Seeding',
                    @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                SELECT @result;
                """, connection, transaction);
            acquire.CommandTimeout = _connections.CommandTimeoutSeconds;
            if ((int)(await acquire.ExecuteScalarAsync(cancellationToken))! < 0)
                throw new InvalidOperationException("Could not acquire the database seeding lock.");

            var before = await ReadDataAsync(connection, transaction, cancellationToken);
            var skipped = demo && before.RowCounts["conference_halls"] > 0;
            var inserted = skipped ? 0 : demo
                ? await DemoDataSeeder.SeedAsync(connection, transaction, _connections.CommandTimeoutSeconds, cancellationToken)
                : await ReferenceDataSeeder.SeedAsync(connection, transaction, _connections.CommandTimeoutSeconds, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            // Report inserts only after the transaction commits; disposal rolls back failures and cancellation.
            return new SeedResult(before, inserted, skipped);

        }
        catch (SqlException exception)
        {
            throw _connections.Translate(exception, "DatabaseSeeder.SeedAsync", cancellationToken);
        }
    }
}
