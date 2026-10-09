IF TYPE_ID(N'[TymchenkoOV].[BookingApp.RoleIds]') IS NULL
    EXEC(N'CREATE TYPE [TymchenkoOV].[BookingApp.RoleIds] AS TABLE (Id int NOT NULL PRIMARY KEY)');
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.user_create]
    @Id uniqueidentifier, @FirstName nvarchar(100), @LastName nvarchar(100), @Email nvarchar(320), @RoleIds [TymchenkoOV].[BookingApp.RoleIds] READONLY AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0 THROW 51002, 'Caller-owned transactions are not supported.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT [TymchenkoOV].[BookingApp.Users](Id, first_name, last_name, email) VALUES (@Id, @FirstName, @LastName, @Email);
        INSERT [TymchenkoOV].[BookingApp.UserRoles](user_id, role_id)
            SELECT DISTINCT @Id, Id FROM @RoleIds;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH;
END;
