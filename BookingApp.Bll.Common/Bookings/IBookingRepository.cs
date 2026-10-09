using BookingApp.Bll.Common.Bookings.Models;

namespace BookingApp.Bll.Common.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(Guid conferenceHallId, DateRange duration, CancellationToken cancellationToken = default);
    IAsyncEnumerable<IReadOnlyCollection<Booking>> ListAsync(int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Booking>> GetListPaginatedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletionAsync(DateTime utcNow, int pageSize, CancellationToken cancellationToken = default);
    /// <summary>Atomically reserves the hall and updates its timestamp. Only Reserved bookings block occupancy.</summary>
    Task<ReservationOutcome> CreateReservationAsync(Booking booking, CancellationToken cancellationToken = default);
    /// <summary>Atomically completes at most pageSize due reservations and returns the actual count.</summary>
    Task<int> CompleteDueAsync(DateTime utcNow, int pageSize, CancellationToken cancellationToken = default);
}
