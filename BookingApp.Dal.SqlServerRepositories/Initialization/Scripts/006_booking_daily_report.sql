CREATE OR ALTER PROCEDURE [TymchenkoOV].[BookingApp.booking_daily_report]
    @StartUtc datetime2(7), @EndUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    IF @StartUtc IS NULL OR @EndUtc IS NULL OR @StartUtc >= @EndUtc
        THROW 51001, 'Invalid reporting period.', 1;
    SELECT b.Id, h.name, b.[start], b.[end], b.status,
        b.total_price_amount, b.total_price_currency
    FROM [TymchenkoOV].[BookingApp.Bookings] b
    JOIN [TymchenkoOV].[BookingApp.ConferenceHalls] h ON h.Id = b.conference_hall_id
    WHERE b.[start] < @EndUtc AND b.[end] > @StartUtc
    ORDER BY b.[start], b.Id;
END;
GO
