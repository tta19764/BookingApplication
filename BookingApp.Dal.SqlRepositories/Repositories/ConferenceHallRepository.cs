using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class ConferenceHallRepository(ApplicationDbContext db, EntityChangeTracker tracker, IMapper mapper) : Repository<ConferenceHallEntity, ConferenceHall>(db, tracker, mapper), IConferenceHallRepository
{
    public async Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHalls(DateRange range, Capacity seats, CancellationToken ct = default) { var unavailable = DbContext.Set<BookingEntity>().Where(x => x.Status == BookingStatus.Reserved && x.Duration.Start < range.End && x.Duration.End > range.Start).Select(x => x.ConferenceHallId); var items = await DbSet.AsNoTracking().Where(x => !unavailable.Contains(x.Id) && x.Seats.Value >= seats.Value).OrderBy(x => x.Name).ToListAsync(ct); return items.Select(ToModel); }
    protected override IQueryable<ConferenceHallEntity> Ordered(IQueryable<ConferenceHallEntity> q) => q.OrderBy(x => x.Id); protected override Guid GetEntityId(ConferenceHallEntity x) => x.Id; protected override Guid GetModelId(ConferenceHall x) => x.Id;
}
