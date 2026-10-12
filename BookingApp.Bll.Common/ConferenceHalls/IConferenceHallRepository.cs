using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Common.ConferenceHalls;

public interface IConferenceHallRepository
{
    /// <summary>Finds a conference hall by its identifier.</summary>
    /// <param name="id">Identifier of the requested conference hall.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The matching item, or null when it does not exist.</returns>
    Task<ConferenceHall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new conference hall.</summary>
    /// <param name="hall">Conference hall details.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>A task completed after the item is persisted.</returns>
    Task AddAsync(ConferenceHall hall, CancellationToken cancellationToken = default);

    /// <summary>Updates editable hall details and returns whether the hall exists.</summary>
    /// <param name="hall">Conference hall details.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>True if the hall was updated; false if it no longer exists.</returns>
    Task<bool> UpdateAsync(ConferenceHall hall, CancellationToken cancellationToken = default);

    /// <summary>Gets one page of persisted conference halls.</summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Maximum number of items per page or batch.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The items in the requested page.</returns>
    Task<IReadOnlyCollection<ConferenceHall>> GetListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Deletes a hall and returns the removal outcome.</summary>
    /// <param name="id">Identifier of the requested conference hall.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>The deletion outcome, including missing halls and reservation conflicts.</returns>
    Task<HallRemovalOutcome> RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Finds persisted halls meeting the capacity and rental period requirements.</summary>
    /// <param name="dateRange">Requested rental period in UTC.</param>
    /// <param name="seats">Minimum number of seats.</param>
    /// <param name="cancellationToken">Cancels asynchronous database access.</param>
    /// <returns>Halls meeting the capacity and availability requirements.</returns>
    Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHallsAsync(DateRange dateRange, Capacity seats, CancellationToken cancellationToken = default);
}
