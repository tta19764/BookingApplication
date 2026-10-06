using System.Data;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Infrastructure;

/// <summary>Creates stored-procedure commands with explicitly typed parameters.</summary>
internal static class SqlProcedure
{
    /// <summary>Creates a stored-procedure command on the supplied connection.</summary>
    /// <param name="connection">An open connection owned by the caller.</param>
    /// <param name="name">The schema-qualified procedure name.</param>
    /// <param name="timeout">The command timeout in seconds.</param>
    /// <returns>A command that the caller must dispose.</returns>
    public static SqlCommand Create(SqlConnection connection, string name, int timeout) => new(name, connection)
    {
        CommandType = CommandType.StoredProcedure,
        CommandTimeout = timeout
    };

    /// <summary>Adds an explicitly typed input parameter, converting null to SQL NULL.</summary>
    /// <param name="command">The target command.</param>
    /// <param name="name">The parameter name including @.</param>
    /// <param name="type">The SQL parameter type.</param>
    /// <param name="value">The input value, or null.</param>
    /// <param name="size">The explicit size; -1 means MAX and zero leaves the provider default.</param>
    /// <returns>The added parameter.</returns>
    public static SqlParameter Parameter(this SqlCommand command, string name, SqlDbType type, object? value,
        int size = 0)
    {
        var parameter = command.Parameters.Add(name, type);
        if (size != 0) parameter.Size = size;
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }

    /// <summary>Adds a decimal(18,2) money parameter.</summary>
    /// <param name="command">The target command.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The monetary amount.</param>
    public static void Decimal(this SqlCommand command, string name, decimal value)
    {
        var parameter = command.Parameter(name, SqlDbType.Decimal, value);
        parameter.Precision = 18;
        parameter.Scale = 2;
    }

    /// <summary>Adds a datetime2(7) parameter after checking UTC timestamp kind.</summary>
    /// <param name="command">The target command.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The UTC timestamp.</param>
    /// <exception cref="ArgumentException">The timestamp kind is not UTC.</exception>
    public static void Utc(this SqlCommand command, string name, DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Database timestamps must be UTC.", name);
        command.Parameter(name, SqlDbType.DateTime2, value).Scale = 7;
    }

    /// <summary>Adds an integer output parameter.</summary>
    /// <param name="command">The target command.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The parameter whose value is available after command execution.</returns>
    public static SqlParameter Output(this SqlCommand command, string name)
    {
        var parameter = command.Parameters.Add(name, SqlDbType.Int);
        parameter.Direction = ParameterDirection.Output;
        return parameter;
    }

    /// <summary>Validates and adds one-based paging parameters.</summary>
    /// <param name="command">The target command.</param>
    /// <param name="page">The positive one-based page number.</param>
    /// <param name="pageSize">The positive maximum number of rows.</param>
    /// <exception cref="ArgumentOutOfRangeException">Page or page size is not positive.</exception>
    public static void Page(this SqlCommand command, int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        command.Parameter("@Page", SqlDbType.Int, page);
        command.Parameter("@PageSize", SqlDbType.Int, pageSize);
    }
}
