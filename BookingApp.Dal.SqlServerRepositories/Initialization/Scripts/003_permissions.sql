-- Setup permissions only. Reference and demo data are inserted by DatabaseSeeder.
IF DATABASE_PRINCIPAL_ID(N'TymchenkoOV.BookingApp.Runtime') IS NULL
    CREATE ROLE [TymchenkoOV.BookingApp.Runtime] AUTHORIZATION dbo;
GRANT EXECUTE ON SCHEMA::[TymchenkoOV] TO [TymchenkoOV.BookingApp.Runtime];
