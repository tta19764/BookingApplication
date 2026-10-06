using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Common.ConferenceHalls;

public interface IConferenceHallRepository
{
    Task<ConferenceHall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ConferenceHall hall, CancellationToken cancellationToken = default);
    /// <summary>Updates editable fields only; returns false if the hall no longer exists.</summary>
    Task<bool> UpdateAsync(ConferenceHall hall, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ConferenceHall>> GetListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<HallRemovalOutcome> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHallsAsync(DateRange dateRange, Capacity seats, CancellationToken cancellationToken = default);
}
