using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Infrastructure;

/// <summary>Creates short-lived pooled connections. No connection is shared between operations.</summary>
public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;
    internal ILogger Logger { get; }
    internal Exception Translate(SqlException exception, string operation, CancellationToken token) =>
        SqlFailure.Translate(exception, operation, Logger, token);
    /// <summary>Gets the positive command execution timeout, in seconds.</summary>
    public int CommandTimeoutSeconds { get; }

    /// <summary>Validates the connection configuration without connecting to SQL Server.</summary>
    /// <param name="connectionString">The externally supplied SQL Server connection string.</param>
    /// <param name="commandTimeoutSeconds">The positive command timeout in seconds.</param>
    /// <param name="logger">Required diagnostic logger, supplied directly by the caller or DI.</param>
    /// <exception cref="ArgumentException">The connection string is empty or malformed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The command timeout is not positive.</exception>
    public SqlConnectionFactory(string connectionString, ILogger logger, int commandTimeoutSeconds = 30)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Configure ConnectionStrings:Database for the remote SQL Server.", nameof(connectionString));
        _ = new SqlConnectionStringBuilder(connectionString);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(commandTimeoutSeconds);
        _connectionString = connectionString;
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
        CommandTimeoutSeconds = commandTimeoutSeconds;
    }

    /// <summary>Opens a new logical connection using SqlClient connection pooling.</summary>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>An open connection that the caller must dispose.</returns>
    /// <remarks>If opening fails or is cancelled, the connection is disposed before the exception propagates.</remarks>
    public async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch (SqlException exception)
        {
            await connection.DisposeAsync();
            throw Translate(exception, "OpenConnection", cancellationToken);
        }
        catch
        {
            // Ownership transfers to the caller only after a successful open.
            await connection.DisposeAsync();
            throw;
        }
    }
}
