using BookingApp.Domain.Bookings;

namespace BookingApp.Domain.ConferenceHalls;

/// <summary>
/// Provides persistence operations for conference halls.
/// </summary>
public interface IConferenceHallRepository
{
    /// <summary>
    /// Finds a hall by its identifier.
    /// </summary>
    Task<ConferenceHall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new hall to the persistence context.
    /// </summary>
    void Add(ConferenceHall hall);

    /// <summary>
    /// Marks an existing hall as changed in the persistence context.
    /// </summary>
    void Update(ConferenceHall hall);

    /// <summary>
    /// Returns one page of halls ordered by identifier.
    /// </summary>
    Task<IReadOnlyCollection<ConferenceHall>> GetListPaginatedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a hall from the persistence context.
    /// </summary>
    void Remove(ConferenceHall hall);

    /// <summary>
    /// Returns halls that can seat the requested capacity and have no overlapping bookings.
    /// </summary>
    Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHalls(DateRange dateRange, Capacity seats, CancellationToken cancellationToken = default);
}
