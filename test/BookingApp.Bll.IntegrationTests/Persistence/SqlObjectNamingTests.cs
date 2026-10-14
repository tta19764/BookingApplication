using BookingApp.Bll.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace BookingApp.Bll.IntegrationTests.Persistence;

[Collection("SqlServer")]
public sealed class SqlObjectNamingTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task FreshInitialization_CreatesOnlyPrefixedApplicationObjects()
    {
        var token = TestContext.Current.CancellationToken;
        await using var connection = new SqlConnection(factory.AdminConnectionString);
        await connection.OpenAsync(token);
        await using var command = new SqlCommand("""
            SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID(N'TymchenkoOV');
            SELECT name FROM sys.procedures WHERE schema_id=SCHEMA_ID(N'TymchenkoOV');
            SELECT version FROM [TymchenkoOV].[BookingApp.SchemaVersions];
            SELECT SCHEMA_ID(N'booking_api'), DATABASE_PRINCIPAL_ID(N'booking_runtime'),
                OBJECT_ID(N'dbo.users'), OBJECT_ID(N'dbo.booking_schema_versions');
            """, connection);
        await using var reader = await command.ExecuteReaderAsync(token);
        var tables = new List<string>();
        while (await reader.ReadAsync(token)) tables.Add(reader.GetString(0));
        tables.Should().NotBeEmpty().And.OnlyContain(name => name.StartsWith("BookingApp.", StringComparison.Ordinal));
        await reader.NextResultAsync(token);
        var procedures = new List<string>();
        while (await reader.ReadAsync(token)) procedures.Add(reader.GetString(0));
        procedures.Should().NotBeEmpty().And.OnlyContain(name => name.StartsWith("BookingApp.", StringComparison.Ordinal));
        await reader.NextResultAsync(token);
        var versions = new List<string>();
        while (await reader.ReadAsync(token)) versions.Add(reader.GetString(0));
        var expectedVersions = typeof(BookingApp.Dal.SqlServerRepositories.Initialization.DatabaseInitializer)
            .Assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".sql", StringComparison.Ordinal))
            .Select(name => name["BookingApp.Dal.SqlServerRepositories.Initialization.Scripts.".Length..]);
        versions.Should().BeEquivalentTo(expectedVersions);
        await reader.NextResultAsync(token); await reader.ReadAsync(token);
        for (var column = 0; column < 4; column++) reader.IsDBNull(column).Should().BeTrue();
    }
}
