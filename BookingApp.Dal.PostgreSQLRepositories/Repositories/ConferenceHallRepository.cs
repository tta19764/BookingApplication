using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class ConferenceHallRepository(ApplicationDbContext dbContext, IMapper mapper)
    : Repository<ConferenceHallEntity, ConferenceHall>(dbContext, mapper), IConferenceHallRepository
{
    public async Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHallsAsync(
        DateRange dateRange,
        Capacity capacity,
        CancellationToken cancellationToken = default)
    {
        var unavailableHallIds = DbContext
            .Set<BookingEntity>()
            .Where(booking => booking.Status == BookingStatus.Reserved &&
                              booking.Duration.Start < dateRange.End &&
                              booking.Duration.End > dateRange.Start)
            .Select(booking => booking.ConferenceHallId);

        var entities = await DbSet
            .AsNoTracking()
            .Where(hall => !unavailableHallIds.Contains(hall.Id) && hall.Seats.Value >= capacity.Value)
            .OrderBy(hall => hall.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(ToModel);
    }

    protected override IQueryable<ConferenceHallEntity> Ordered(IQueryable<ConferenceHallEntity> query) =>
        query.OrderBy(hall => hall.Id);

    protected override Guid GetEntityId(ConferenceHallEntity entity) => entity.Id;

    protected override Guid GetModelId(ConferenceHall model) => model.Id;
}
