-- SQL Server 2019+; run in the intended database, not master.
-- The deployment runner journals this baseline. Existing unrelated tables are not adopted.
IF OBJECT_ID(N'[TymchenkoOV].[BookingApp.ConferenceHalls]', N'U') IS NOT NULL OR OBJECT_ID(N'[TymchenkoOV].[BookingApp.Bookings]', N'U') IS NOT NULL
   OR OBJECT_ID(N'[TymchenkoOV].[BookingApp.Users]', N'U') IS NOT NULL OR OBJECT_ID(N'[TymchenkoOV].[BookingApp.Roles]', N'U') IS NOT NULL
   OR OBJECT_ID(N'[TymchenkoOV].[BookingApp.Permissions]', N'U') IS NOT NULL OR OBJECT_ID(N'[TymchenkoOV].[BookingApp.UserRoles]', N'U') IS NOT NULL
   OR OBJECT_ID(N'[TymchenkoOV].[BookingApp.RolePermissions]', N'U') IS NOT NULL
    THROW 51000, 'Application tables already exist without this migration baseline; review the schema before adoption.', 1;
GO
IF SCHEMA_ID(N'TymchenkoOV') IS NULL EXEC(N'CREATE SCHEMA [TymchenkoOV] AUTHORIZATION dbo');
GO
CREATE TABLE [TymchenkoOV].[BookingApp.ConferenceHalls] (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_conference_halls PRIMARY KEY,
    name nvarchar(100) NOT NULL,
    capacity int NOT NULL CONSTRAINT CK_hall_capacity CHECK (capacity > 0),
    hourly_rate decimal(18,2) NOT NULL CONSTRAINT CK_hall_rate CHECK (hourly_rate > 0),
    currency nvarchar(3) NOT NULL CONSTRAINT CK_hall_currency CHECK (currency = N'UAH'),
    last_booked_on_utc datetime2(7) NULL,
    amenities nvarchar(100) NOT NULL
);
CREATE TABLE [TymchenkoOV].[BookingApp.Users] (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_users PRIMARY KEY,
    first_name nvarchar(100) NOT NULL,
    last_name nvarchar(100) NOT NULL,
    email nvarchar(320) NOT NULL CONSTRAINT UQ_user_email UNIQUE
);
CREATE TABLE [TymchenkoOV].[BookingApp.Roles] (
    Id int NOT NULL CONSTRAINT PK_roles PRIMARY KEY,
    name nvarchar(100) NOT NULL CONSTRAINT UQ_role_name UNIQUE
);
CREATE TABLE [TymchenkoOV].[BookingApp.Permissions] (
    Id int NOT NULL CONSTRAINT PK_permissions PRIMARY KEY,
    name nvarchar(100) NOT NULL CONSTRAINT UQ_permission_name UNIQUE
);
CREATE TABLE [TymchenkoOV].[BookingApp.RolePermissions] (
    role_id int NOT NULL REFERENCES [TymchenkoOV].[BookingApp.Roles](Id),
    permission_id int NOT NULL REFERENCES [TymchenkoOV].[BookingApp.Permissions](Id),
    CONSTRAINT PK_role_permissions PRIMARY KEY (role_id, permission_id)
);
CREATE TABLE [TymchenkoOV].[BookingApp.UserRoles] (
    user_id uniqueidentifier NOT NULL REFERENCES [TymchenkoOV].[BookingApp.Users](Id),
    role_id int NOT NULL REFERENCES [TymchenkoOV].[BookingApp.Roles](Id),
    CONSTRAINT PK_user_roles PRIMARY KEY (user_id, role_id)
);
CREATE TABLE [TymchenkoOV].[BookingApp.Bookings] (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_bookings PRIMARY KEY,
    conference_hall_id uniqueidentifier NOT NULL CONSTRAINT FK_booking_hall REFERENCES [TymchenkoOV].[BookingApp.ConferenceHalls](Id),
    user_id uniqueidentifier NOT NULL CONSTRAINT FK_booking_user REFERENCES [TymchenkoOV].[BookingApp.Users](Id),
    [start] datetime2(7) NOT NULL,
    [end] datetime2(7) NOT NULL,
    price_for_period_amount decimal(18,2) NOT NULL,
    price_for_period_currency nvarchar(3) NOT NULL,
    amenities_up_charge_amount decimal(18,2) NOT NULL,
    amenities_up_charge_currency nvarchar(3) NOT NULL,
    total_price_amount decimal(18,2) NOT NULL,
    total_price_currency nvarchar(3) NOT NULL,
    status nvarchar(50) NOT NULL,
    created_on_utc datetime2(7) NOT NULL,
    rejected_on_utc datetime2(7) NULL,
    completed_on_utc datetime2(7) NULL,
    cancelled_on_utc datetime2(7) NULL,
    CONSTRAINT CK_booking_period CHECK ([start] < [end]),
    CONSTRAINT CK_booking_status CHECK (status IN (N'Reserved', N'Rejected', N'Cancelled', N'Completed')),
    CONSTRAINT CK_booking_price CHECK (price_for_period_amount >= 0 AND amenities_up_charge_amount >= 0
        AND total_price_amount = price_for_period_amount + amenities_up_charge_amount),
    CONSTRAINT CK_booking_currency CHECK (price_for_period_currency = N'UAH'
        AND amenities_up_charge_currency = N'UAH' AND total_price_currency = N'UAH')
);
CREATE INDEX IX_booking_occupancy ON [TymchenkoOV].[BookingApp.Bookings](conference_hall_id, status, [start]) INCLUDE ([end]);
CREATE INDEX IX_booking_due ON [TymchenkoOV].[BookingApp.Bookings]([end], Id) WHERE status = N'Reserved';
CREATE INDEX IX_booking_user ON [TymchenkoOV].[BookingApp.Bookings](user_id);
CREATE INDEX IX_user_roles_role ON [TymchenkoOV].[BookingApp.UserRoles](role_id);
CREATE INDEX IX_role_permissions_permission ON [TymchenkoOV].[BookingApp.RolePermissions](permission_id);
