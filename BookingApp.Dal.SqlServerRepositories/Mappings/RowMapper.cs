using BookingApp.Dal.SqlServerRepositories.Entities;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Mappings;

/// <summary>Hydrates DAL entities only. Business conversion is owned by the AutoMapper profile.</summary>
internal static class RowMapper
{
    private static string Text(SqlDataReader row, string column) => row.GetString(row.GetOrdinal(column));
    private static Guid Id(SqlDataReader row, string column) => row.GetGuid(row.GetOrdinal(column));
    private static decimal Amount(SqlDataReader row, string column) => row.GetDecimal(row.GetOrdinal(column));
    private static DateTime Utc(SqlDataReader row, string column) => DateTime.SpecifyKind(row.GetDateTime(row.GetOrdinal(column)), DateTimeKind.Utc);
    private static DateTime? OptionalUtc(SqlDataReader row, string column) => row.IsDBNull(row.GetOrdinal(column)) ? null : Utc(row, column);

    /// <summary>Hydrates a UserEntity from the current result row.</summary>
    /// <param name="row">A reader positioned on a row with the procedure's named columns.</param>
    /// <returns>The DAL entity without advancing the reader.</returns>
    /// <remarks>Relationship assembly is handled by the repository.</remarks>
    public static UserEntity User(SqlDataReader row) => new()
    {
        Id = Id(row, "Id"),
        FirstName = Text(row, "first_name"),
        LastName = Text(row, "last_name"),
        Email = Text(row, "email")
    };

    /// <summary>Hydrates a RoleEntity from the current result row.</summary>
    /// <param name="row">A reader positioned on a row with the procedure's named columns.</param>
    /// <returns>The DAL entity without advancing the reader.</returns>
    /// <remarks>Relationship assembly is handled by the repository.</remarks>
    public static RoleEntity Role(SqlDataReader row) => new()
    {
        Id = row.GetInt32(row.GetOrdinal("Id")),
        Name = Text(row, "name")
    };

    /// <summary>Hydrates a PermissionEntity from the current result row.</summary>
    /// <param name="row">A reader positioned on a row with the procedure's named columns.</param>
    /// <returns>The DAL entity without advancing the reader.</returns>
    /// <remarks>Relationship assembly is handled by the repository.</remarks>
    public static PermissionEntity Permission(SqlDataReader row) => new()
    {
        Id = row.GetInt32(row.GetOrdinal("Id")),
        Name = Text(row, "name")
    };

    /// <summary>Hydrates a ConferenceHallEntity from the current result row.</summary>
    /// <param name="row">A reader positioned on a row with the procedure's named columns.</param>
    /// <returns>The DAL entity without advancing the reader.</returns>
    /// <remarks>SQL datetime2 values are restored as UTC timestamps.</remarks>
    public static ConferenceHallEntity Hall(SqlDataReader row) => new()
    {
        Id = Id(row, "Id"),
        Name = Text(row, "name"),
        Capacity = row.GetInt32(row.GetOrdinal("capacity")),
        HourlyRate = Amount(row, "hourly_rate"),
        Currency = Text(row, "currency"),
        Amenities = Text(row, "amenities"),
        LastBookedOnUtc = OptionalUtc(row, "last_booked_on_utc")
    };

    /// <summary>Hydrates a BookingEntity from the current result row.</summary>
    /// <param name="row">A reader positioned on a row with the procedure's named columns.</param>
    /// <returns>The DAL entity without advancing the reader.</returns>
    /// <remarks>SQL datetime2 values are restored as UTC timestamps.</remarks>
    public static BookingEntity Booking(SqlDataReader row) => new()
    {
        Id = Id(row, "Id"),
        ConferenceHallId = Id(row, "conference_hall_id"),
        UserId = Id(row, "user_id"),
        Start = Utc(row, "start"), End = Utc(row, "end"),
        PriceForPeriodAmount = Amount(row, "price_for_period_amount"),
        PriceForPeriodCurrency = Text(row, "price_for_period_currency"),
        AmenitiesUpChargeAmount = Amount(row, "amenities_up_charge_amount"),
        AmenitiesUpChargeCurrency = Text(row, "amenities_up_charge_currency"),
        TotalPriceAmount = Amount(row, "total_price_amount"),
        TotalPriceCurrency = Text(row, "total_price_currency"),
        Status = Text(row, "status"),
        CreatedOnUtc = Utc(row, "created_on_utc"),
        RejectedOnUtc = OptionalUtc(row, "rejected_on_utc"),
        CompletedOnUtc = OptionalUtc(row, "completed_on_utc"),
        CancelledOnUtc = OptionalUtc(row, "cancelled_on_utc")
    };
}
