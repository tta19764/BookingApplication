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
            SELECT COUNT(*) FROM [TymchenkoOV].[BookingApp.SchemaVersions];
            SELECT SCHEMA_ID(N'booking_api'), DATABASE_PRINCIPAL_ID(N'booking_runtime'),
                OBJECT_ID(N'dbo.users'), OBJECT_ID(N'dbo.booking_schema_versions');
            """, connection);
        await using var reader = await command.ExecuteReaderAsync(token);
        var tables = new List<string>();
        while (await reader.ReadAsync(token)) tables.Add(reader.GetString(0));
        tables.Should().BeEquivalentTo(new[] { "ConferenceHalls", "Users", "Roles", "Permissions",
            "RolePermissions", "UserRoles", "Bookings", "SchemaVersions" }.Select(name => "BookingApp." + name));
        await reader.NextResultAsync(token);
        var procedures = new List<string>();
        while (await reader.ReadAsync(token)) procedures.Add(reader.GetString(0));
        procedures.Should().BeEquivalentTo(new[] { "hall_get", "hall_list", "hall_available", "hall_create",
            "hall_update", "hall_delete", "booking_get", "booking_list", "booking_has_overlap", "booking_reserve",
            "booking_due", "booking_complete_due", "user_get", "user_create" }.Select(name => "BookingApp." + name));
        await reader.NextResultAsync(token); await reader.ReadAsync(token);
        reader.GetInt32(0).Should().Be(3);
        await reader.NextResultAsync(token); await reader.ReadAsync(token);
        for (var column = 0; column < 4; column++) reader.IsDBNull(column).Should().BeTrue();
    }
}
