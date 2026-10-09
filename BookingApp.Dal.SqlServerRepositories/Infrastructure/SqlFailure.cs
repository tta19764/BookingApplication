using BookingApp.Bll.Common.Shared.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BookingApp.Dal.SqlServerRepositories.Infrastructure;

internal static class SqlFailure
{
    internal static PersistenceException Translate(SqlException exception, string operation, ILogger? logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var error = exception.Number switch
        {
            -2 => PersistenceError.Timeout,
            53 or 64 or 233 or 4060 or 10053 or 10054 or 10060 or 11001 => PersistenceError.Unavailable,
            229 or 262 or 18456 => PersistenceError.AccessDenied,
            2601 or 2627 or 547 => PersistenceError.ConstraintViolation,
            208 or 2812 => PersistenceError.SchemaMismatch,
            51010 => PersistenceError.Conflict,
            _ => PersistenceError.Failure
        };
        var incidentId = Guid.NewGuid();
        // Do not log SQL text, parameter values, credentials or the provider message (which may contain row data).
        logger?.LogError("Persistence failure {IncidentId}: {Operation}, {Error}, SQL number {SqlNumber}, state {SqlState}, class {SqlClass}",
            incidentId, operation, error, exception.Number, exception.State, exception.Class);
        return new PersistenceException(error, operation, incidentId);
    }
}
