using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Initialization;

/// <summary>Inserts the optional hall catalog inside the caller-owned setup transaction.</summary>
internal static class DemoDataSeeder
{
    internal static async Task<int> SeedAsync(SqlConnection connection, SqlTransaction transaction,
        int timeout, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SET NOCOUNT ON;
            SET XACT_ABORT ON;
            INSERT dbo.conference_halls(Id,name,capacity,hourly_rate,currency,amenities) VALUES
                ('11111111-1111-1111-1111-111111111111',N'Hall A',50,2000,N'UAH',N'1,2,3'),
                ('22222222-2222-2222-2222-222222222222',N'Hall B',100,3500,N'UAH',N'1,2,3'),
                ('33333333-3333-3333-3333-333333333333',N'Hall C',30,1500,N'UAH',N'1,2,3');
            SELECT @@ROWCOUNT;
            """, connection, transaction);
        command.CommandTimeout = timeout;
        return (int)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
