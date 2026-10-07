-- Setup permissions only. Reference and demo data are inserted by DatabaseSeeder.
IF DATABASE_PRINCIPAL_ID(N'booking_runtime') IS NULL CREATE ROLE booking_runtime AUTHORIZATION dbo;
GRANT EXECUTE ON SCHEMA::booking_api TO booking_runtime;
