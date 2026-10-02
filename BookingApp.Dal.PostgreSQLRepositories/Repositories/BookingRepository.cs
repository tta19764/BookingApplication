using System.Runtime.CompilerServices;
using AutoMapper;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class BookingRepository(ApplicationDbContext dbContext, IMapper mapper)
    : Repository<BookingEntity, Booking>(dbContext, mapper), IBookingRepository
{
    public Task<bool> HasOverlapAsync(
        Guid conferenceHallId,
        DateRange duration,
        CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            booking => booking.ConferenceHallId == conferenceHallId &&
                       booking.Duration.Start < duration.End &&
                       booking.Duration.End > duration.Start,
            cancellationToken);
    }

    public async IAsyncEnumerable<IReadOnlyCollection<Booking>> ListAsync(
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var page = 0; ; page++)
        {
            var entities = await DbSet
                .AsNoTracking()
                .OrderBy(booking => booking.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            if (entities.Count == 0)
            {
                yield break;
            }

            yield return entities.Select(ToModel).ToList();
        }
    }

    public async Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletionAsync(
        DateTime utcNow,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var entities = await DbSet
            .Where(booking => booking.Status == BookingStatus.Reserved && booking.Duration.End <= utcNow)
            .OrderBy(booking => booking.Duration.End)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return entities.Select(ToModel).ToList();
    }

    protected override IQueryable<BookingEntity> Ordered(IQueryable<BookingEntity> query) =>
        query.OrderBy(booking => booking.Id);

    protected override Guid GetEntityId(BookingEntity entity) => entity.Id;

    protected override Guid GetModelId(Booking model) => model.Id;
}
