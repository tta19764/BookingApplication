-- All identifiers are fixed; no caller-supplied SQL. Read schemas are part of the DAL contract.
-- Write procedures own their transactions and must not be invoked in ambient transactions.
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.hall_get] @Id uniqueidentifier AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, name, capacity, hourly_rate, currency, last_booked_on_utc, amenities
    FROM [TymchenkoOV].[BookingApp.ConferenceHalls] WHERE Id = @Id;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.hall_list] @Page int, @PageSize int AS
BEGIN
    SET NOCOUNT ON;
    IF @Page <= 0 OR @PageSize <= 0 THROW 51001, 'Invalid pagination.', 1;
    SELECT Id, name, capacity, hourly_rate, currency, last_booked_on_utc, amenities
    FROM [TymchenkoOV].[BookingApp.ConferenceHalls] ORDER BY Id
    OFFSET (CONVERT(bigint, @Page) - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.hall_available] @Start datetime2(7), @End datetime2(7), @Capacity int AS
BEGIN
    SET NOCOUNT ON;
    IF @Start >= @End OR @Capacity <= 0 THROW 51001, 'Invalid availability criteria.', 1;
    SELECT h.Id, h.name, h.capacity, h.hourly_rate, h.currency, h.last_booked_on_utc, h.amenities
    FROM [TymchenkoOV].[BookingApp.ConferenceHalls] h WHERE h.capacity >= @Capacity
    AND NOT EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.Bookings] b WHERE b.conference_hall_id = h.Id
        AND b.status = N'Reserved' AND b.[start] < @End AND b.[end] > @Start)
    ORDER BY h.name, h.Id;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.hall_create]
    @Id uniqueidentifier, @Name nvarchar(100), @Capacity int, @HourlyRate decimal(18,2),
    @Currency nvarchar(3), @Amenities nvarchar(100) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    INSERT [TymchenkoOV].[BookingApp.ConferenceHalls](Id, name, capacity, hourly_rate, currency, amenities)
    VALUES (@Id, @Name, @Capacity, @HourlyRate, @Currency, @Amenities);
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.hall_update]
    @Id uniqueidentifier, @Name nvarchar(100), @Capacity int, @HourlyRate decimal(18,2),
    @Currency nvarchar(3), @Amenities nvarchar(100), @Updated int OUTPUT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    -- Do not overwrite last_booked_on_utc from a stale application model.
    UPDATE [TymchenkoOV].[BookingApp.ConferenceHalls] SET name = @Name, capacity = @Capacity, hourly_rate = @HourlyRate,
        currency = @Currency, amenities = @Amenities WHERE Id = @Id;
    SET @Updated = @@ROWCOUNT;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.hall_delete] @Id uniqueidentifier, @Outcome int OUTPUT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0 THROW 51002, 'Caller-owned transactions are not supported.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @LockResult int, @Resource nvarchar(255) = N'BookingHall:' + LOWER(CONVERT(nvarchar(36), @Id));
        EXEC @LockResult = sys.sp_getapplock @Resource = @Resource, @LockMode = 'Exclusive',
            @LockOwner = 'Transaction', @LockTimeout = 10000;
        IF @LockResult < 0 THROW 51003, 'Could not acquire hall reservation lock.', 1;
        IF NOT EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.ConferenceHalls] WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id)
        BEGIN SET @Outcome = 1; ROLLBACK; RETURN; END;
        IF EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.Bookings] WHERE conference_hall_id = @Id)
        BEGIN SET @Outcome = 2; ROLLBACK; RETURN; END;
        DELETE [TymchenkoOV].[BookingApp.ConferenceHalls] WHERE Id = @Id;
        SET @Outcome = 0;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_get] @Id uniqueidentifier AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, conference_hall_id, user_id, [start], [end], price_for_period_amount, price_for_period_currency,
        amenities_up_charge_amount, amenities_up_charge_currency, total_price_amount, total_price_currency,
        status, created_on_utc, rejected_on_utc, completed_on_utc, cancelled_on_utc
    FROM [TymchenkoOV].[BookingApp.Bookings] WHERE Id = @Id;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_list] @Page int, @PageSize int AS
BEGIN
    SET NOCOUNT ON;
    IF @Page <= 0 OR @PageSize <= 0 THROW 51001, 'Invalid pagination.', 1;
    SELECT Id, conference_hall_id, user_id, [start], [end], price_for_period_amount, price_for_period_currency,
        amenities_up_charge_amount, amenities_up_charge_currency, total_price_amount, total_price_currency,
        status, created_on_utc, rejected_on_utc, completed_on_utc, cancelled_on_utc
    FROM [TymchenkoOV].[BookingApp.Bookings] ORDER BY Id
    OFFSET (CONVERT(bigint, @Page) - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_has_overlap]
    @HallId uniqueidentifier, @Start datetime2(7), @End datetime2(7) AS
BEGIN
    SET NOCOUNT ON;
    IF @Start >= @End THROW 51001, 'Invalid period.', 1;
    SELECT CONVERT(bit, CASE WHEN EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.Bookings] WHERE conference_hall_id = @HallId
        AND status = N'Reserved' AND [start] < @End AND [end] > @Start) THEN 1 ELSE 0 END);
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_reserve]
    @Id uniqueidentifier, @HallId uniqueidentifier, @UserId uniqueidentifier,
    @Start datetime2(7), @End datetime2(7), @CreatedOnUtc datetime2(7),
    @PriceForPeriod decimal(18,2), @AmenitiesUpCharge decimal(18,2), @TotalPrice decimal(18,2),
    @PriceCurrency nvarchar(3), @AmenitiesCurrency nvarchar(3), @TotalCurrency nvarchar(3), @Outcome int OUTPUT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0 THROW 51002, 'Caller-owned transactions are not supported.', 1;
    IF @Start >= @End THROW 51001, 'Invalid period.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @LockResult int, @Resource nvarchar(255) = N'BookingHall:' + LOWER(CONVERT(nvarchar(36), @HallId));
        EXEC @LockResult = sys.sp_getapplock @Resource = @Resource, @LockMode = 'Exclusive',
            @LockOwner = 'Transaction', @LockTimeout = 10000;
        IF @LockResult < 0 THROW 51003, 'Could not acquire hall reservation lock.', 1;
        IF NOT EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.ConferenceHalls] WITH (UPDLOCK, HOLDLOCK) WHERE Id = @HallId)
        BEGIN SET @Outcome = 2; ROLLBACK; RETURN; END;
        IF NOT EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.Users] WITH (HOLDLOCK) WHERE Id = @UserId)
        BEGIN SET @Outcome = 3; ROLLBACK; RETURN; END;
        IF EXISTS (SELECT 1 FROM [TymchenkoOV].[BookingApp.Bookings] WHERE conference_hall_id = @HallId
            AND status = N'Reserved' AND [start] < @End AND [end] > @Start)
        BEGIN SET @Outcome = 1; ROLLBACK; RETURN; END;

        -- Both writes roll back on any failure, including PK, money or FK constraint violations.
        UPDATE [TymchenkoOV].[BookingApp.ConferenceHalls] SET last_booked_on_utc =
            CASE WHEN last_booked_on_utc > @CreatedOnUtc THEN last_booked_on_utc ELSE @CreatedOnUtc END
            WHERE Id = @HallId;
        INSERT [TymchenkoOV].[BookingApp.Bookings](Id, conference_hall_id, user_id, [start], [end], price_for_period_amount,
            price_for_period_currency, amenities_up_charge_amount, amenities_up_charge_currency,
            total_price_amount, total_price_currency, status, created_on_utc)
        VALUES (@Id, @HallId, @UserId, @Start, @End, @PriceForPeriod, @PriceCurrency,
            @AmenitiesUpCharge, @AmenitiesCurrency, @TotalPrice, @TotalCurrency, N'Reserved', @CreatedOnUtc);
        SET @Outcome = 0;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_due] @UtcNow datetime2(7), @PageSize int AS
BEGIN
    SET NOCOUNT ON;
    IF @PageSize <= 0 THROW 51001, 'Invalid batch size.', 1;
    SELECT TOP (@PageSize) Id, conference_hall_id, user_id, [start], [end], price_for_period_amount,
        price_for_period_currency, amenities_up_charge_amount, amenities_up_charge_currency,
        total_price_amount, total_price_currency, status, created_on_utc, rejected_on_utc, completed_on_utc, cancelled_on_utc
    FROM [TymchenkoOV].[BookingApp.Bookings] WHERE status = N'Reserved' AND [end] <= @UtcNow ORDER BY [end], Id;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_complete_due]
    @UtcNow datetime2(7), @PageSize int, @CompletedCount int OUTPUT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0 THROW 51002, 'Caller-owned transactions are not supported.', 1;
    IF @PageSize <= 0 THROW 51001, 'Invalid batch size.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        ;WITH due AS (
            SELECT TOP (@PageSize) status, completed_on_utc
            FROM [TymchenkoOV].[BookingApp.Bookings] WITH (UPDLOCK) WHERE status = N'Reserved' AND [end] <= @UtcNow ORDER BY [end], Id
        )
        UPDATE due SET status = N'Completed', completed_on_utc = @UtcNow;
        SET @CompletedCount = @@ROWCOUNT;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.user_get] @Id uniqueidentifier AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, first_name, last_name, email FROM [TymchenkoOV].[BookingApp.Users] WHERE Id = @Id;
    SELECT r.Id, r.name FROM [TymchenkoOV].[BookingApp.Roles] r JOIN [TymchenkoOV].[BookingApp.UserRoles] ur ON ur.role_id = r.Id WHERE ur.user_id = @Id ORDER BY r.Id;
    SELECT rp.role_id, p.Id, p.name FROM [TymchenkoOV].[BookingApp.UserRoles] ur
        JOIN [TymchenkoOV].[BookingApp.RolePermissions] rp ON rp.role_id = ur.role_id
        JOIN [TymchenkoOV].[BookingApp.Permissions] p ON p.Id = rp.permission_id WHERE ur.user_id = @Id ORDER BY rp.role_id, p.Id;
END;
GO
CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.user_create]
    @Id uniqueidentifier, @FirstName nvarchar(100), @LastName nvarchar(100), @Email nvarchar(320), @RoleIds nvarchar(max) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT <> 0 THROW 51002, 'Caller-owned transactions are not supported.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT [TymchenkoOV].[BookingApp.Users](Id, first_name, last_name, email) VALUES (@Id, @FirstName, @LastName, @Email);
        INSERT [TymchenkoOV].[BookingApp.UserRoles](user_id, role_id)
            SELECT DISTINCT @Id, CONVERT(int, value) FROM STRING_SPLIT(@RoleIds, ',');
        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        THROW;
    END CATCH;
END;
