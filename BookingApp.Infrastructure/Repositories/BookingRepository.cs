using BookingApp.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for booking persistence and booking-specific queries.
/// </summary>
public class BookingRepository(ApplicationDbContext dbContext)
    : Repository<Booking>(dbContext), IBookingRepository
{
    /// <summary>
    /// Returns true when the hall has any booking that intersects the requested period.
    /// </summary>
    public async Task<bool> HasOverlap(
        Guid conferenceHallId,
        DateRange duration,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            booking =>
                booking.ConferenceHallId == conferenceHallId &&
                booking.Duration.Start < duration.End &&
                booking.Duration.End > duration.Start,
            cancellationToken);
    }

    public async IAsyncEnumerable<IReadOnlyCollection<Booking>> List(
        int pageSize,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
        }

        var page = 0;

        while (true)
        {
            // Read deterministic pages so reporting code can aggregate without loading the full table.
            var bookings = await DbSet
                .AsNoTracking()
                .OrderBy(booking => booking.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (bookings.Count == 0)
            {
                yield break;
            }

            yield return bookings;

            page++;
        }
    }

    /// <summary>
    /// Gets a bounded batch of reserved bookings whose end time has passed.
    /// </summary>
    public async Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletion(
        DateTime utcNow,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
        {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
        }

        return await DbSet
            .Where(booking =>
                booking.Status == BookingStatus.Reserved &&
                booking.Duration.End <= utcNow)
            .OrderBy(booking => booking.Duration.End)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
