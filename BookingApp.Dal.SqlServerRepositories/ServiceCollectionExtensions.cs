using Microsoft.Extensions.Logging;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Users;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApp.Dal.SqlServerRepositories;

/// <summary>Registers the SQL Server persistence implementation.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the connection factory and scoped repository implementations.</summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="connectionString">The remote SQL Server runtime connection string.</param>
    /// <param name="commandTimeoutSeconds">The positive command timeout in seconds.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <remarks>The composition root must also register the DAL AutoMapperConfig profile. This method performs no schema changes or seeding.</remarks>
    public static IServiceCollection AddSqlServerDataAccess(this IServiceCollection services,
        string connectionString, int commandTimeoutSeconds = SqlConnectionFactory.DefaultCommandTimeoutSeconds)
    {
        services.AddSqlServerReportingDataAccess(connectionString, commandTimeoutSeconds);
        services.AddScoped<IConferenceHallRepository, ConferenceHallRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }

    /// <summary>Registers only the connection factory and read-only booking report repository.</summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="connectionString">SQL Server reporting connection string.</param>
    /// <param name="commandTimeoutSeconds">Positive command timeout in seconds.</param>
    /// <returns>The same collection for chaining.</returns>
    /// <remarks>Requires logging, but no AutoMapper registration. Performs no database initialization.</remarks>
    public static IServiceCollection AddSqlServerReportingDataAccess(this IServiceCollection services,
        string connectionString, int commandTimeoutSeconds = SqlConnectionFactory.DefaultCommandTimeoutSeconds)
    {
        services.AddSingleton(provider => new SqlConnectionFactory(connectionString,
            provider.GetRequiredService<ILogger<SqlConnectionFactory>>(), commandTimeoutSeconds));
        services.AddScoped<IBookingReportRepository, BookingReportRepository>();
        return services;
    }
}
