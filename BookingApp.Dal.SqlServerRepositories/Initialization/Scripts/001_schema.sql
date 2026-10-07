-- SQL Server 2019+; run in the intended database, not master.
-- The deployment runner journals this baseline. Existing unrelated tables are not adopted.
IF OBJECT_ID(N'dbo.conference_halls', N'U') IS NOT NULL OR OBJECT_ID(N'dbo.bookings', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.users', N'U') IS NOT NULL OR OBJECT_ID(N'dbo.roles', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.permissions', N'U') IS NOT NULL OR OBJECT_ID(N'dbo.user_roles', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.role_permissions', N'U') IS NOT NULL
    THROW 51000, 'Application tables already exist without this migration baseline; review the schema before adoption.', 1;
GO
IF SCHEMA_ID(N'booking_api') IS NULL EXEC(N'CREATE SCHEMA booking_api AUTHORIZATION dbo');
GO
CREATE TABLE dbo.conference_halls (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_conference_halls PRIMARY KEY,
    name nvarchar(100) NOT NULL,
    capacity int NOT NULL CONSTRAINT CK_hall_capacity CHECK (capacity > 0),
    hourly_rate decimal(18,2) NOT NULL CONSTRAINT CK_hall_rate CHECK (hourly_rate > 0),
    currency nvarchar(3) NOT NULL CONSTRAINT CK_hall_currency CHECK (currency = N'UAH'),
    last_booked_on_utc datetime2(7) NULL,
    amenities nvarchar(100) NOT NULL
);
CREATE TABLE dbo.users (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_users PRIMARY KEY,
    first_name nvarchar(100) NOT NULL,
    last_name nvarchar(100) NOT NULL,
    email nvarchar(320) NOT NULL CONSTRAINT UQ_user_email UNIQUE
);
CREATE TABLE dbo.roles (
    Id int NOT NULL CONSTRAINT PK_roles PRIMARY KEY,
    name nvarchar(100) NOT NULL CONSTRAINT UQ_role_name UNIQUE
);
CREATE TABLE dbo.permissions (
    Id int NOT NULL CONSTRAINT PK_permissions PRIMARY KEY,
    name nvarchar(100) NOT NULL CONSTRAINT UQ_permission_name UNIQUE
);
CREATE TABLE dbo.role_permissions (
    role_id int NOT NULL REFERENCES dbo.roles(Id),
    permission_id int NOT NULL REFERENCES dbo.permissions(Id),
    CONSTRAINT PK_role_permissions PRIMARY KEY (role_id, permission_id)
);
CREATE TABLE dbo.user_roles (
    user_id uniqueidentifier NOT NULL REFERENCES dbo.users(Id),
    role_id int NOT NULL REFERENCES dbo.roles(Id),
    CONSTRAINT PK_user_roles PRIMARY KEY (user_id, role_id)
);
CREATE TABLE dbo.bookings (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_bookings PRIMARY KEY,
    conference_hall_id uniqueidentifier NOT NULL CONSTRAINT FK_booking_hall REFERENCES dbo.conference_halls(Id),
    user_id uniqueidentifier NOT NULL CONSTRAINT FK_booking_user REFERENCES dbo.users(Id),
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
CREATE INDEX IX_booking_occupancy ON dbo.bookings(conference_hall_id, status, [start]) INCLUDE ([end]);
CREATE INDEX IX_booking_due ON dbo.bookings([end], Id) WHERE status = N'Reserved';
CREATE INDEX IX_booking_user ON dbo.bookings(user_id);
CREATE INDEX IX_user_roles_role ON dbo.user_roles(role_id);
CREATE INDEX IX_role_permissions_permission ON dbo.role_permissions(permission_id);
