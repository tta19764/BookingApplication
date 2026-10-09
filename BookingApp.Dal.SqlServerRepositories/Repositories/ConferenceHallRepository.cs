using AutoMapper;
using BookingApp.Dal.SqlServerRepositories.Entities;
using System.Data;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Mappings;
using Microsoft.Data.SqlClient;

namespace BookingApp.Dal.SqlServerRepositories.Repositories;

/// <summary>Persists conference hall operations through SQL Server stored procedures.</summary>
/// <param name="connections">Creates a separate pooled connection for each operation.</param>
/// <param name="mapper">Converts DAL entities to and from business models.</param>
public sealed class ConferenceHallRepository(SqlConnectionFactory connections, IMapper mapper) : IConferenceHallRepository
{
    /// <summary>Reads a conference hall by its identifier.</summary>
    /// <param name="id">The conference hall identifier.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>The conference hall, or null when it does not exist.</returns>
    /// <remarks>Related bookings are not loaded.</remarks>
    public async Task<ConferenceHall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync<ConferenceHall?>(connections, "[TymchenkoOV].[BookingApp.hall_get]",
            "ConferenceHallRepository.GetByIdAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, id);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? mapper.Map<ConferenceHall>(RowMapper.Hall(reader)) : null;
        }, cancellationToken);
    }

    /// <summary>Creates a hall and persists its editable fields immediately.</summary>
    /// <param name="hall">The hall to create; its last-booked timestamp is managed by reservation procedures.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>A task that completes after the database write.</returns>
    public Task AddAsync(ConferenceHall hall, CancellationToken cancellationToken = default) => WriteAsync(hall, false, cancellationToken);

    /// <summary>Updates hall catalog fields while preserving its database booking timestamp.</summary>
    /// <param name="hall">The identifier and replacement editable values.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>True when updated; false when the hall no longer exists.</returns>
    public async Task<bool> UpdateAsync(ConferenceHall hall, CancellationToken cancellationToken = default) =>
        await WriteAsync(hall, true, cancellationToken);

    private async Task<bool> WriteAsync(ConferenceHall hall, bool update, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<ConferenceHallEntity>(hall);
        return await SqlProcedure.ExecuteAsync(connections, update ? "[TymchenkoOV].[BookingApp.hall_update]" : "[TymchenkoOV].[BookingApp.hall_create]",
            "ConferenceHallRepository.WriteAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, entity.Id);
            command.Parameter("@Name", SqlDbType.NVarChar, entity.Name, 100);
            command.Parameter("@Capacity", SqlDbType.Int, entity.Capacity);
            command.Decimal("@HourlyRate", entity.HourlyRate);
            command.Parameter("@Currency", SqlDbType.NVarChar, entity.Currency, 3);
            command.Parameter("@Amenities", SqlDbType.NVarChar, entity.Amenities, 100);
            // LastBookedOnUtc is deliberately excluded so stale catalog edits cannot overwrite reservations.
            var outcome = update ? command.Output("@Updated") : null;
            await command.ExecuteNonQueryAsync(cancellationToken);
            return outcome is null || (int)outcome.Value == 1;
        }, cancellationToken);
    }

    /// <summary>Deletes a hall only when no booking references it.</summary>
    /// <param name="id">The hall identifier.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Removed, NotFound, or HasBookings as reported by the procedure.</returns>
    /// <remarks>Deletion shares the reservation procedure's per-hall lock to coordinate concurrent writes.</remarks>
    /// <exception cref="InvalidDataException">The procedure returns an unknown outcome.</exception>
    public async Task<HallRemovalOutcome> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.hall_delete]",
            "ConferenceHallRepository.RemoveAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, id);
            var outcome = command.Output("@Outcome");
            await command.ExecuteNonQueryAsync(cancellationToken);
            var result = (HallRemovalOutcome)(int)outcome.Value;
            return Enum.IsDefined(result) ? result : throw new InvalidDataException("Unknown hall removal outcome");
        }, cancellationToken);
    }

    /// <summary>Reads one deterministically ordered page of halls.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The positive maximum number of rows.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Materialized halls; an empty collection when no rows remain.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Page or page size is not positive.</exception>
    public async Task<IReadOnlyCollection<ConferenceHall>> GetListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.hall_list]",
            "ConferenceHallRepository.GetListPaginatedAsync", async command =>
        {
            command.Page(page, pageSize);
            return await ReadAsync(command, cancellationToken);
        }, cancellationToken);
    }

    /// <summary>Reads halls with sufficient capacity and no overlapping reservation.</summary>
    /// <param name="dateRange">The requested half-open UTC period.</param>
    /// <param name="seats">The minimum required capacity.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>Materialized available halls.</returns>
    /// <remarks>Availability is advisory; reservation creation rechecks occupancy atomically.</remarks>
    public async Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHallsAsync(DateRange dateRange, Capacity seats, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.hall_available]",
            "ConferenceHallRepository.GetAvailableConferenceHallsAsync", async command =>
        {
            command.Utc("@Start", dateRange.Start);
            command.Utc("@End", dateRange.End);
            command.Parameter("@Capacity", SqlDbType.Int, seats.Value);
            return await ReadAsync(command, cancellationToken);
        }, cancellationToken);
    }

    private async Task<IReadOnlyCollection<ConferenceHall>> ReadAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var halls = new List<ConferenceHall>();
        while (await reader.ReadAsync(cancellationToken)) halls.Add(mapper.Map<ConferenceHall>(RowMapper.Hall(reader)));
        return halls;
    }
}
