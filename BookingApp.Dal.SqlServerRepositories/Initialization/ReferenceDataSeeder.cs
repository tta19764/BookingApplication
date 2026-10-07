using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Initialization;

/// <summary>Adds missing required catalog rows without replacing existing data.</summary>
internal static class ReferenceDataSeeder
{
    internal static async Task<int> SeedAsync(SqlConnection connection, SqlTransaction transaction,
        int timeout, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SET NOCOUNT ON;
            SET XACT_ABORT ON;
            DECLARE @inserted int=0;
            DECLARE @permissions TABLE(Id int, name nvarchar(100));
            INSERT @permissions VALUES (1,N'conference-halls:read'),(2,N'conference-halls:write'),
                (3,N'bookings:read'),(4,N'bookings:write');
            IF EXISTS (SELECT 1 FROM dbo.roles WHERE (Id=1 AND name<>N'Registered') OR (name=N'Registered' AND Id<>1))
                THROW 51010, 'Reference role conflicts with existing data; no rows were seeded.', 1;
            IF EXISTS (SELECT 1 FROM dbo.permissions p JOIN @permissions v ON p.Id=v.Id OR p.name=v.name
                WHERE p.Id<>v.Id OR p.name<>v.name)
                THROW 51010, 'Reference permissions conflict with existing data; no rows were seeded.', 1;
            IF EXISTS (SELECT 1 FROM dbo.users WHERE
                (Id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' AND email<>N'seeded.user@booking.local')
                OR (email=N'seeded.user@booking.local' AND Id<>'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'))
                THROW 51010, 'Reference user conflicts with existing data; no rows were seeded.', 1;

            INSERT dbo.roles(Id,name) SELECT 1,N'Registered'
                WHERE NOT EXISTS (SELECT 1 FROM dbo.roles WHERE Id=1);
            SET @inserted+=@@ROWCOUNT;
            INSERT dbo.permissions(Id,name) SELECT v.Id,v.name FROM @permissions v
                WHERE NOT EXISTS (SELECT 1 FROM dbo.permissions p WHERE p.Id=v.Id);
            SET @inserted+=@@ROWCOUNT;
            INSERT dbo.role_permissions(role_id,permission_id) SELECT 1,v.Id FROM @permissions v
                WHERE NOT EXISTS (SELECT 1 FROM dbo.role_permissions rp WHERE rp.role_id=1 AND rp.permission_id=v.Id);
            SET @inserted+=@@ROWCOUNT;
            INSERT dbo.users(Id,first_name,last_name,email)
                SELECT 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',N'Seeded',N'User',N'seeded.user@booking.local'
                WHERE NOT EXISTS (SELECT 1 FROM dbo.users WHERE Id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa');
            SET @inserted+=@@ROWCOUNT;
            INSERT dbo.user_roles(user_id,role_id) SELECT 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',1
                WHERE NOT EXISTS (SELECT 1 FROM dbo.user_roles WHERE user_id='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' AND role_id=1);
            SET @inserted+=@@ROWCOUNT;
            SELECT @inserted;
            """, connection, transaction);
        command.CommandTimeout = timeout;
        return (int)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
