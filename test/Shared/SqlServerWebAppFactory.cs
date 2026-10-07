using BookingApp.Dal.SqlServerRepositories.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace BookingApp.IntegrationTests.Infrastructure;

/// <summary>Uses the real deployment scripts and an EXECUTE-only application identity in an ephemeral SQL Server.</summary>
public class SqlServerWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder(
        Environment.GetEnvironmentVariable("BOOKINGAPP_TEST_SQL_IMAGE")
        ?? "mcr.microsoft.com/mssql/server@sha256:2dca9ee5cd5316952d9b6ef4a0c088ac95b55e3502accdda0fc12ad6ede7b905").Build();

    public string AdminConnectionString { get; private set; } = string.Empty;
    public string RuntimeConnectionString { get; private set; } = string.Empty;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Minimal hosting needs these settings before Program registers the DAL and scheduler.
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = RuntimeConnectionString,
                ["BackgroundJobs:CompleteBookings:Enabled"] = "false",
                ["DatabaseSeeding:Enabled"] = "false"
            }));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = RuntimeConnectionString,
                ["BackgroundJobs:CompleteBookings:Enabled"] = "false",
                ["DatabaseSeeding:Enabled"] = "false"
            }));
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            await _container.StartAsync(timeout.Token);
            var password = "TestOnly!" + Guid.NewGuid().ToString("N");
            await using (var connection = new SqlConnection(_container.GetConnectionString()))
            {
                await connection.OpenAsync(timeout.Token);
                await using var setup = new SqlCommand($"""
                    CREATE DATABASE BookingAppTests;
                    CREATE LOGIN booking_test_runtime WITH PASSWORD=N'{password}', CHECK_POLICY=OFF;
                    """, connection);
                await setup.ExecuteNonQueryAsync(timeout.Token);
            }
            var admin = new SqlConnectionStringBuilder(_container.GetConnectionString()) { InitialCatalog = "BookingAppTests" };
            AdminConnectionString = admin.ConnectionString;
            await DatabaseInitializer.ApplyAsync(AdminConnectionString, timeout.Token);
            var seeder = new DatabaseSeeder(AdminConnectionString);
            await seeder.SeedReferenceDataAsync(timeout.Token);
            await seeder.SeedDemoDataAsync(timeout.Token);
            await using (var connection = new SqlConnection(AdminConnectionString))
            {
                await connection.OpenAsync(timeout.Token);
                await using var grants = new SqlCommand("""
                    CREATE USER booking_test_runtime FOR LOGIN booking_test_runtime;
                    ALTER ROLE booking_runtime ADD MEMBER booking_test_runtime;
                    """, connection);
                await grants.ExecuteNonQueryAsync(timeout.Token);
            }
            var runtime = new SqlConnectionStringBuilder(AdminConnectionString)
            {
                UserID = "booking_test_runtime", Password = password
            };
            RuntimeConnectionString = runtime.ConnectionString;
        }
        catch
        {
            await _container.DisposeAsync();
            throw;
        }
    }

    public new async ValueTask DisposeAsync()
    {
        try { await base.DisposeAsync(); }
        finally { await _container.DisposeAsync(); }
    }
}
