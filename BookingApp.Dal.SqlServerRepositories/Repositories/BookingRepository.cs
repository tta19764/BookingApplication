using AutoMapper;
using BookingApp.Dal.SqlServerRepositories.Entities;
using System.Data;
using System.Runtime.CompilerServices;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Mappings;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Repositories;

/// <summary>Persists booking operations through SQL Server stored procedures.</summary>
/// <param name="connections">Creates a separate pooled connection for each operation.</param>
/// <param name="mapper">Converts DAL entities to and from business models.</param>
public sealed class BookingRepository(SqlConnectionFactory connections, IMapper mapper) : IBookingRepository
{
    /// <summary>Reads a booking by its identifier.</summary>
    /// <param name="id">The booking identifier.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>The booking, or null when it does not exist.</returns>
    /// <remarks>Related bookings are not loaded.</remarks>
    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync<Booking?>(connections, "[TymchenkoOV].[BookingApp.booking_get]",
            "BookingRepository.GetByIdAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, id);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? mapper.Map<Booking>(RowMapper.Booking(reader)) : null;
        }, cancellationToken);
    }

    /// <summary>Checks whether a reserved booking overlaps the requested half-open period.</summary>
    /// <param name="conferenceHallId">The hall to check.</param>
    /// <param name="duration">The UTC period; adjacent bookings do not overlap.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>True when a blocking reservation exists.</returns>
    /// <remarks>This check is advisory. CreateReservationAsync performs the authoritative check under a database lock.</remarks>
    public async Task<bool> HasOverlapAsync(Guid conferenceHallId, DateRange duration, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.booking_has_overlap]",
            "BookingRepository.HasOverlapAsync", async command =>
        {
            command.Parameter("@HallId", SqlDbType.UniqueIdentifier, conferenceHallId);
            command.Utc("@Start", duration.Start);
            command.Utc("@End", duration.End);
            return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
        }, cancellationToken);
    }

    /// <summary>Atomically creates a reservation and updates the hall booking timestamp.</summary>
    /// <param name="booking">A Reserved booking with UTC timestamps and calculated price snapshots.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Created, Overlap, HallNotFound, or UserNotFound as reported by the procedure.</returns>
    /// <remarks>The procedure owns the transaction and per-hall concurrency lock. Infrastructure failures propagate rather than becoming business outcomes.</remarks>
    /// <exception cref="ArgumentException">The booking status is not Reserved or a timestamp is not UTC.</exception>
    /// <exception cref="InvalidDataException">The procedure returns an unknown outcome.</exception>
    public async Task<ReservationOutcome> CreateReservationAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        if (booking.Status != BookingStatus.Reserved) throw new ArgumentException("Only reservations can be created.", nameof(booking));
        var entity = mapper.Map<BookingEntity>(booking);
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.booking_reserve]",
            "BookingRepository.CreateReservationAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, entity.Id);
            command.Parameter("@HallId", SqlDbType.UniqueIdentifier, entity.ConferenceHallId);
            command.Parameter("@UserId", SqlDbType.UniqueIdentifier, entity.UserId);
            command.Utc("@Start", entity.Start);
            command.Utc("@End", entity.End);
            command.Utc("@CreatedOnUtc", entity.CreatedOnUtc);
            command.Decimal("@PriceForPeriod", entity.PriceForPeriodAmount);
            command.Decimal("@AmenitiesUpCharge", entity.AmenitiesUpChargeAmount);
            command.Decimal("@TotalPrice", entity.TotalPriceAmount);
            command.Parameter("@PriceCurrency", SqlDbType.NVarChar, entity.PriceForPeriodCurrency, 3);
            command.Parameter("@AmenitiesCurrency", SqlDbType.NVarChar, entity.AmenitiesUpChargeCurrency, 3);
            command.Parameter("@TotalCurrency", SqlDbType.NVarChar, entity.TotalPriceCurrency, 3);
            // The procedure commits both writes before reporting a business outcome.
            var outcome = command.Output("@Outcome");
            await command.ExecuteNonQueryAsync(cancellationToken);
            var result = (ReservationOutcome)(int)outcome.Value;
            return Enum.IsDefined(result) ? result : throw new InvalidDataException("Unknown reservation outcome");
        }, cancellationToken);
    }

    /// <summary>Reads one deterministically ordered page of bookings.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The positive maximum number of rows.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Materialized bookings; an empty collection when no rows remain.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Page or page size is not positive.</exception>
    public async Task<IReadOnlyCollection<Booking>> GetListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.booking_list]",
            "BookingRepository.GetListPaginatedAsync", async command =>
        {
            command.Page(page, pageSize);
            return await ReadAsync(command, cancellationToken);
        }, cancellationToken);
    }

    /// <summary>Lists booking pages until the first empty page.</summary>
    /// <param name="pageSize">The positive maximum number of bookings per page.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Materialized pages, each fetched using its own connection.</returns>
    /// <remarks>Enumeration does not create a database snapshot; concurrent writes may change later pages.</remarks>
    public async IAsyncEnumerable<IReadOnlyCollection<Booking>> ListAsync(int pageSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        for (var page = 1; ; page = checked(page + 1))
        {
            var bookings = await GetListPaginatedAsync(page, pageSize, cancellationToken);
            if (bookings.Count == 0) yield break;
            yield return bookings;
        }
    }

    /// <summary>Reads a bounded batch of reservations whose rental period has ended.</summary>
    /// <param name="utcNow">The UTC cutoff for identifying due reservations.</param>
    /// <param name="pageSize">The positive maximum batch size.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Due reservations without changing their status.</returns>
    /// <remarks>Use CompleteDueAsync for an atomic status transition; this read does not claim rows.</remarks>
    public async Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletionAsync(DateTime utcNow, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.booking_due]",
            "BookingRepository.GetReservedBookingsDueForCompletionAsync", async command =>
        {
            command.Utc("@UtcNow", utcNow);
            command.Parameter("@PageSize", SqlDbType.Int, pageSize);
            return await ReadAsync(command, cancellationToken);
        }, cancellationToken);
    }

    /// <summary>Atomically completes a bounded batch of due reservations.</summary>
    /// <param name="utcNow">The UTC cutoff, also recorded as the completion timestamp.</param>
    /// <param name="pageSize">The positive maximum batch size.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>The actual number of reservations transitioned to Completed.</returns>
    /// <remarks>Guarded database updates allow repeat calls and competing workers without completing a reservation twice.</remarks>
    public async Task<int> CompleteDueAsync(DateTime utcNow, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.booking_complete_due]",
            "BookingRepository.CompleteDueAsync", async command =>
        {
            command.Utc("@UtcNow", utcNow);
            command.Parameter("@PageSize", SqlDbType.Int, pageSize);
            // NOCOUNT makes ExecuteNonQuery row counts unsuitable; use the explicit transition count.
            var count = command.Output("@CompletedCount");
            await command.ExecuteNonQueryAsync(cancellationToken);
            return (int)count.Value;
        }, cancellationToken);
    }

    private async Task<IReadOnlyCollection<Booking>> ReadAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var bookings = new List<Booking>();
        while (await reader.ReadAsync(cancellationToken)) bookings.Add(mapper.Map<Booking>(RowMapper.Booking(reader)));
        return bookings;
    }
}
