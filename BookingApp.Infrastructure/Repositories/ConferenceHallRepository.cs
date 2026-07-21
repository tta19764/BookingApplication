using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for conference hall persistence and availability queries.
/// </summary>
public class ConferenceHallRepository(ApplicationDbContext dbContext)
    : Repository<ConferenceHall>(dbContext), IConferenceHallRepository
{
    /// <summary>
    /// Returns halls that satisfy capacity and have no reserved overlapping bookings.
    /// </summary>
    public async Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHalls(
        DateRange dateRange,
        Capacity seats,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<ConferenceHall>()
            .AsNoTracking()
            .Where(hall => hall.Seats.Value >= seats.Value)
            // A hall is unavailable only when an active reservation overlaps the requested period.
            .Where(hall => 
                !DbContext.Set<Booking>()
                .Where(booking =>
                    booking.Status == BookingStatus.Reserved &&
                    booking.Duration.Start < dateRange.End &&
                    booking.Duration.End > dateRange.Start)
                .Select(booking => booking.ConferenceHallId).Contains(hall.Id))
            .OrderBy(hall => hall.Name)
            .ToListAsync(cancellationToken);
    }
}
