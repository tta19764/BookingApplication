using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using BookingApp.Bll.Common.Shared.Exceptions;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Initialization;
using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace BookingApp.Bll.IntegrationTests.Persistence;

[Collection("SqlServer")]
public sealed class DatabaseSeederTests(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
    private readonly string _database = "SeederTests_" + Guid.NewGuid().ToString("N");
    private string _connectionString = string.Empty;
    private DatabaseSeeder Seeder => new(_connectionString, NullLogger<DatabaseSeeder>.Instance);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(factory.AdminConnectionString, $"CREATE DATABASE [{_database}]");
        _connectionString = new SqlConnectionStringBuilder(factory.AdminConnectionString) { InitialCatalog = _database }.ConnectionString;
        // Install scripts without seeding application rows.
        await DatabaseInitializer.ApplyAsync(_connectionString, Token);
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup uses a separate admin connection and does not depend on the test cancellation token.
        await ExecuteAsync(factory.AdminConnectionString,
            $"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}];", CancellationToken.None);
    }

    private static async Task ExecuteAsync(string connectionString, string sql, CancellationToken? cancellationToken = null)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken ?? Token);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken ?? Token);
    }

    [Fact]
    public async Task Initialization_SkipsPermissionsByDefaultAndAllowsExplicitSetup()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(Token);
        await using var role = new SqlCommand("SELECT COUNT(*) FROM sys.database_principals WHERE name=N'TymchenkoOV.BookingApp.Runtime'", connection);
        ((int)(await role.ExecuteScalarAsync(Token))!).Should().Be(0);
        await using var journal = new SqlCommand("SELECT COUNT(*) FROM [TymchenkoOV].[BookingApp.SchemaVersions]", connection);
        ((int)(await journal.ExecuteScalarAsync(Token))!).Should().Be(3);

        await DatabaseInitializer.ApplyAsync(_connectionString, Token, includePermissions: true);
        ((int)(await role.ExecuteScalarAsync(Token))!).Should().Be(1);
        ((int)(await journal.ExecuteScalarAsync(Token))!).Should().Be(5);
        await Seeder.InitializeAsync(Token);
        ((int)(await journal.ExecuteScalarAsync(Token))!).Should().Be(5);
    }

    [Fact]
    public async Task Inspection_ReportsAllTablesWithoutWriting()
    {
        var inspection = await Seeder.InspectAsync(Token);
        inspection.HasData.Should().BeFalse();
        inspection.RowCounts.Should().HaveCount(7);
        inspection.RowCounts.Values.Should().OnlyContain(count => count == 0);
    }

    [Fact]
    public async Task MissingTable_ReportsSchemaFailureWithoutCreatingSchema()
    {
        await ExecuteAsync(_connectionString, "DROP TABLE [TymchenkoOV].[BookingApp.Bookings]");
        var action = () => Seeder.SeedReferenceDataAsync(Token);
        using var logs = new CapturingLogger();
        action = () => new DatabaseSeeder(_connectionString, logs).SeedReferenceDataAsync(Token);
        var exception = await action.Should().ThrowAsync<PersistenceException>();
        logs.Messages.Should().ContainSingle().Which.Should().Contain(exception.Which.IncidentId.ToString());
        logs.Messages.Single().Should().NotContain("Invalid object name").And.NotContain(_connectionString);
        exception.Which.Error.Should().Be(PersistenceError.SchemaMismatch);
        exception.Which.InnerException.Should().BeNull();
    }

    [Fact]
    public async Task ReferenceSeeds_FillMissingRowsAndPreserveExistingValues()
    {
        (await Seeder.SeedReferenceDataAsync(Token)).InsertedRows.Should().Be(11);
        await ExecuteAsync(_connectionString, """
            UPDATE [TymchenkoOV].[BookingApp.Users] SET first_name=N'Edited' WHERE Id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
            DELETE [TymchenkoOV].[BookingApp.RolePermissions] WHERE permission_id=4;
            DELETE [TymchenkoOV].[BookingApp.Permissions] WHERE Id=4;
            """);
        var result = await Seeder.SeedReferenceDataAsync(Token);
        result.Before.HasData.Should().BeTrue();
        result.InsertedRows.Should().Be(2);
        (await Seeder.SeedReferenceDataAsync(Token)).InsertedRows.Should().Be(0);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(Token);
        await using var command = new SqlCommand("SELECT first_name FROM [TymchenkoOV].[BookingApp.Users]", connection);
        (await command.ExecuteScalarAsync(Token)).Should().Be("Edited");
    }

    [Fact]
    public async Task DemoSeeds_RunOnEmptyCatalogAndSkipExistingCatalog()
    {
        await Seeder.SeedReferenceDataAsync(Token);
        var initial = await Seeder.SeedDemoDataAsync(Token);
        initial.Before.HasData.Should().BeTrue();
        initial.InsertedRows.Should().Be(3);
        initial.Skipped.Should().BeFalse();
        await ExecuteAsync(_connectionString, "UPDATE [TymchenkoOV].[BookingApp.ConferenceHalls] SET name=N'Edited'; DELETE [TymchenkoOV].[BookingApp.ConferenceHalls] WHERE Id='33333333-3333-3333-3333-333333333333'");
        var repeated = await Seeder.SeedDemoDataAsync(Token);
        repeated.Skipped.Should().BeTrue();
        repeated.InsertedRows.Should().Be(0);
        (await Seeder.InspectAsync(Token)).RowCounts["conference_halls"].Should().Be(2);
    }

    [Fact]
    public async Task CustomCatalog_IsPreservedAndNotSupplementedWithDemoData()
    {
        await ExecuteAsync(_connectionString, """
            INSERT [TymchenkoOV].[BookingApp.ConferenceHalls](Id,name,capacity,hourly_rate,currency,amenities)
            VALUES(NEWID(),N'Custom',20,100,N'UAH',N'1');
            """);
        var result = await Seeder.SeedDemoDataAsync(Token);
        result.Before.HasData.Should().BeTrue();
        result.Skipped.Should().BeTrue();
        (await Seeder.InspectAsync(Token)).RowCounts["conference_halls"].Should().Be(1);
    }

    [Fact]
    public async Task ConflictingReferenceIdentity_IsRejectedWithoutPartialWrites()
    {
        await ExecuteAsync(_connectionString, "INSERT [TymchenkoOV].[BookingApp.Permissions](Id,name) VALUES(1,N'custom:permission')");
        var action = () => Seeder.SeedReferenceDataAsync(Token);
        await action.Should().ThrowAsync<PersistenceException>();
        var state = await Seeder.InspectAsync(Token);
        state.RowCounts["permissions"].Should().Be(1);
        state.RowCounts["roles"].Should().Be(0);
        state.RowCounts["users"].Should().Be(0);
    }

    [Fact]
    public async Task InsertFailure_RollsBackEarlierReferenceWrites()
    {
        await ExecuteAsync(_connectionString, """
            CREATE TRIGGER [TymchenkoOV].[BookingApp.reject_seed_user] ON [TymchenkoOV].[BookingApp.Users] AFTER INSERT AS
            BEGIN THROW 51011, 'Test insertion failure', 1; END;
            """);
        var action = () => Seeder.SeedReferenceDataAsync(Token);
        await action.Should().ThrowAsync<PersistenceException>();
        (await Seeder.InspectAsync(Token)).HasData.Should().BeFalse();
    }

    [Fact]
    public async Task ConcurrentReferenceSeeds_InsertEachRowOnce()
    {
        var results = await Task.WhenAll(Seeder.SeedReferenceDataAsync(Token), Seeder.SeedReferenceDataAsync(Token));
        results.Select(result => result.InsertedRows).Should().BeEquivalentTo([0, 11]);
        (await Seeder.InspectAsync(Token)).RowCounts["user_roles"].Should().Be(1);
    }

    [Fact]
    public async Task RuntimeCredentials_CannotInspectOrSeed()
    {
        var action = () => new DatabaseSeeder(factory.RuntimeConnectionString, NullLogger<DatabaseSeeder>.Instance).SeedReferenceDataAsync(Token);
        var exception = await action.Should().ThrowAsync<PersistenceException>();
        exception.Which.Error.Should().Be(PersistenceError.AccessDenied);
    }
    private sealed class CapturingLogger : ILogger<DatabaseSeeder>, IDisposable
    {
        public List<string> Messages { get; } = [];
        public void Dispose() { }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }
}
