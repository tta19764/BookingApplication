using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;

namespace BookingApp.Dal.SqlServerRepositories.Repositories;

/// <summary>Reads report rows through the dedicated reporting procedure.</summary>
/// <param name="connections">Creates pooled SQL connections for report queries.</param>
public sealed class BookingReportRepository(SqlConnectionFactory connections) : IBookingReportRepository
{
    /// <inheritdoc />
    public Task<IReadOnlyList<BookingReportRow>> GetAsync(DateTime startUtc, DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        if (startUtc >= endUtc) throw new ArgumentException("The reporting period must be positive.");
        return SqlProcedure.ExecuteAsync<IReadOnlyList<BookingReportRow>>(connections,
            "[TymchenkoOV].[BookingApp.booking_daily_report]", "BookingReportRepository.GetAsync",
            async (command, token) =>
            {
                command.Utc("@StartUtc", startUtc);
                command.Utc("@EndUtc", endUtc);
                await using var reader = await command.ExecuteReaderAsync(token);
                var rows = new List<BookingReportRow>();
                while (await reader.ReadAsync(token))
                    rows.Add(new BookingReportRow(reader.GetGuid(reader.GetOrdinal("Id")),
                        reader.GetString(reader.GetOrdinal("name")),
                        DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("start")), DateTimeKind.Utc),
                        DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("end")), DateTimeKind.Utc),
                        reader.GetString(reader.GetOrdinal("status")),
                        reader.GetDecimal(reader.GetOrdinal("total_price_amount")),
                        reader.GetString(reader.GetOrdinal("total_price_currency"))));
                return rows;
            }, cancellationToken);
    }
}

