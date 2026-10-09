using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Initialization;

/// <summary>Explicitly applies embedded schema and procedure scripts, with optional permission setup to an application database.</summary>
/// <remarks>Invoked through IDatabaseSeeder before data seeding when startup seeding is enabled, or explicitly during setup.</remarks>
public static partial class DatabaseInitializer
{
    /// <summary>Applies unapplied initialization scripts transactionally, rejecting changes to already-applied scripts.</summary>
    /// <param name="connectionString">Administrative connection string for the target application database.</param>
    /// <param name="cancellationToken">Cancels connection opening, script execution, and transaction commits.</param>
    /// <param name="logger">Optional DAL diagnostic logger.</param>
    /// <param name="includePermissions">Includes administrative role and grant setup when true. Disabled by default.</param>
    /// <returns>A task that completes after the selected initialization scripts are applied.</returns>
    /// <exception cref="InvalidOperationException">A system database is selected, the initialization lock cannot be acquired, or an applied script checksum changed.</exception>
    public static async Task ApplyAsync(string connectionString,
        CancellationToken cancellationToken = default, ILogger? logger = null, bool includePermissions = false)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            if (connection.Database.Equals("master", StringComparison.OrdinalIgnoreCase)
                || connection.Database.Equals("tempdb", StringComparison.OrdinalIgnoreCase)
                || connection.Database.Equals("model", StringComparison.OrdinalIgnoreCase)
                || connection.Database.Equals("msdb", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Specify a dedicated application database for deployment.");

            // Session ownership permits one transaction per script while retaining the deployment lock.
            await using var acquire = new SqlCommand("""
                DECLARE @result int;
                EXEC @result = sys.sp_getapplock @Resource=N'BookingApplication.Deployment',
                    @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=30000;
                SELECT @result;
                """, connection);
            acquire.CommandTimeout = 60;
            if ((int)(await acquire.ExecuteScalarAsync(cancellationToken))! < 0)
                throw new InvalidOperationException("Could not acquire database deployment lock.");
            try
            {
                await using var journal = new SqlCommand("""
                    SET XACT_ABORT ON;
                    BEGIN TRANSACTION;
                    IF SCHEMA_ID(N'TymchenkoOV') IS NULL EXEC(N'CREATE SCHEMA [TymchenkoOV] AUTHORIZATION dbo');
                    IF OBJECT_ID(N'[TymchenkoOV].[BookingApp.SchemaVersions]', N'U') IS NULL
                        CREATE TABLE [TymchenkoOV].[BookingApp.SchemaVersions](
                            version nvarchar(100) NOT NULL PRIMARY KEY, checksum varchar(64) NOT NULL,
                            applied_on_utc datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME());
                    COMMIT;
                    """, connection);
                await journal.ExecuteNonQueryAsync(cancellationToken);
                var assembly = typeof(DatabaseInitializer).Assembly;
                const string prefix = "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.";
                var scripts = assembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith(prefix, StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.Ordinal))
                    .Where(name => includePermissions || !name.EndsWith("_permissions.sql", StringComparison.Ordinal))
                    .OrderBy(name => name, StringComparer.Ordinal);
                foreach (var resourceName in scripts)
                {
                    var version = resourceName[prefix.Length..];
                    await using var stream = assembly.GetManifestResourceStream(resourceName)!;
                    using var reader = new StreamReader(stream);
                    // Git may check out SQL with CRLF on Windows and LF on Linux; checksums must be identical.
                    var sql = (await reader.ReadToEndAsync(cancellationToken)).Replace("\r\n", "\n");
                    var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
                    await using var applied = new SqlCommand("SELECT checksum FROM [TymchenkoOV].[BookingApp.SchemaVersions] WHERE version=@Version", connection);
                    applied.Parameters.Add("@Version", SqlDbType.NVarChar, 100).Value = version;
                    var previous = await applied.ExecuteScalarAsync(cancellationToken);
                    if (previous is not null)
                    {
                        if (!checksum.Equals((string)previous, StringComparison.Ordinal))
                            throw new InvalidOperationException($"Migration {version} changed after deployment. Add a new migration instead.");
                        continue;
                    }

                    await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
                    foreach (var batch in BatchSeparator().Split(sql).Where(batch => !string.IsNullOrWhiteSpace(batch)))
                    {
                        await using var command = new SqlCommand(batch, connection, transaction);
                        command.CommandTimeout = 60;
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    await using var record = new SqlCommand("INSERT [TymchenkoOV].[BookingApp.SchemaVersions](version, checksum) VALUES (@Version, @Checksum)", connection, transaction);
                    record.Parameters.Add("@Version", SqlDbType.NVarChar, 100).Value = version;
                    record.Parameters.Add("@Checksum", SqlDbType.VarChar, 64).Value = checksum;
                    await record.ExecuteNonQueryAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            finally
            {
                await using var release = new SqlCommand("EXEC sys.sp_releaseapplock @Resource=N'BookingApplication.Deployment', @LockOwner='Session'", connection);
                await release.ExecuteNonQueryAsync(CancellationToken.None);
            }

        }
        catch (SqlException exception)
        {
            throw SqlFailure.Translate(exception, "InitializeDatabase", logger, cancellationToken);
        }
    }

    // Scripts use a standalone GO only. This intentionally does not implement arbitrary sqlcmd syntax.
    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex BatchSeparator();
}
