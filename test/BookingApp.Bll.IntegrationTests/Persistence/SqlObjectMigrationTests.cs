using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BookingApp.Bll.IntegrationTests.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Initialization;
using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace BookingApp.Bll.IntegrationTests.Persistence;

[Collection("SqlServer")]
public sealed class SqlObjectMigrationTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task LegacyUpgrade_PreservesDataJournalAndRoleMembership_AndCanBeRepeated()
    {
        var token = TestContext.Current.CancellationToken;
        var database = "ObjectMigration_" + Guid.NewGuid().ToString("N");
        await using var admin = new SqlConnection(factory.AdminConnectionString);
        await admin.OpenAsync(token);
        await using var create = new SqlCommand($"CREATE DATABASE [{database}]", admin);
        await create.ExecuteNonQueryAsync(token);
        var connectionString = new SqlConnectionStringBuilder(factory.AdminConnectionString) { InitialCatalog = database }.ConnectionString;
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(token);
            await using var journal = new SqlCommand("""
                CREATE TABLE dbo.booking_schema_versions(
                    version nvarchar(100) NOT NULL PRIMARY KEY, checksum varchar(64) NOT NULL,
                    applied_on_utc datetime2(7) NOT NULL DEFAULT SYSUTCDATETIME());
                """, connection);
            await journal.ExecuteNonQueryAsync(token);
            foreach (var version in new[] { "001_schema.sql", "002_stored_procedures.sql", "003_permissions.sql" })
            {
                await using var stream = typeof(DatabaseInitializer).Assembly.GetManifestResourceStream(
                    "BookingApp.Dal.SqlServerRepositories.Initialization.Scripts." + version)!;
                using var reader = new StreamReader(stream);
                var sql = (await reader.ReadToEndAsync(token)).Replace("\r\n", "\n");
                foreach (var batch in Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                    .Where(batch => !string.IsNullOrWhiteSpace(batch)))
                {
                    await using var command = new SqlCommand(batch, connection);
                    await command.ExecuteNonQueryAsync(token);
                }
                await using var record = new SqlCommand("INSERT dbo.booking_schema_versions(version,checksum) VALUES(@Version,@Checksum)", connection);
                record.Parameters.Add("@Version", SqlDbType.NVarChar, 100).Value = version;
                record.Parameters.Add("@Checksum", SqlDbType.VarChar, 64).Value = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
                await record.ExecuteNonQueryAsync(token);
            }
            await using var legacy = new SqlCommand("""
                CREATE TABLE dbo.Unrelated(Id int);
                INSERT dbo.Unrelated VALUES(42);
                CREATE USER migration_runtime WITHOUT LOGIN;
                ALTER ROLE booking_runtime ADD MEMBER migration_runtime;
                INSERT dbo.users(Id,first_name,last_name,email)
                    VALUES('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',N'Preserved',N'User',N'preserved@example.local');
                INSERT dbo.conference_halls(Id,name,capacity,hourly_rate,currency,amenities)
                    VALUES('11111111-1111-1111-1111-111111111111',N'Preserved hall',10,100,N'UAH',N'');
                INSERT dbo.bookings(Id,conference_hall_id,user_id,[start],[end],price_for_period_amount,
                    price_for_period_currency,amenities_up_charge_amount,amenities_up_charge_currency,
                    total_price_amount,total_price_currency,status,created_on_utc)
                    VALUES('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb','11111111-1111-1111-1111-111111111111',
                    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','2026-10-14T10:00:00','2026-10-14T11:00:00',
                    100,N'UAH',0,N'UAH',100,N'UAH',N'Reserved',SYSUTCDATETIME());
                """, connection);
            await legacy.ExecuteNonQueryAsync(token);

            await DatabaseInitializer.ApplyAsync(connectionString, token);
            await DatabaseInitializer.ApplyAsync(connectionString, token);
            await using var verify = new SqlCommand("""
                SELECT COUNT(*) FROM [TymchenkoOV].[BookingApp.SchemaVersions];
                SELECT first_name FROM [TymchenkoOV].[BookingApp.Users];
                SELECT COUNT(*) FROM [TymchenkoOV].[BookingApp.Bookings] b
                    JOIN [TymchenkoOV].[BookingApp.ConferenceHalls] h ON h.Id=b.conference_hall_id
                    JOIN [TymchenkoOV].[BookingApp.Users] u ON u.Id=b.user_id;
                SELECT IS_ROLEMEMBER(N'TymchenkoOV.BookingApp.Runtime',N'migration_runtime');
                SELECT COUNT(*) FROM sys.procedures WHERE schema_id=SCHEMA_ID(N'TymchenkoOV') AND name LIKE N'BookingApp.%';
                SELECT COUNT(*) FROM sys.tables WHERE schema_id=SCHEMA_ID(N'TymchenkoOV') AND name LIKE N'BookingApp.%';
                SELECT COUNT(*) FROM sys.procedures WHERE schema_id=SCHEMA_ID(N'booking_api');
                SELECT Id FROM dbo.Unrelated;
                """, connection);
            await using var result = await verify.ExecuteReaderAsync(token);
            await result.ReadAsync(token); result.GetInt32(0).Should().Be(4);
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetString(0).Should().Be("Preserved");
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetInt32(0).Should().Be(1);
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetInt32(0).Should().Be(1);
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetInt32(0).Should().Be(14);
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetInt32(0).Should().Be(8);
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetInt32(0).Should().Be(0);
            await result.NextResultAsync(token); await result.ReadAsync(token); result.GetInt32(0).Should().Be(42);
        }
        finally
        {
            await using var cleanup = new SqlCommand($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]", admin);
            await cleanup.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }
}
