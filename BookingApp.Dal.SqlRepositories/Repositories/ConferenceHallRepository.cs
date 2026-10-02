using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class ConferenceHallRepository(ApplicationDbContext db, EntityChangeTracker tracker) : Repository<ConferenceHallEntity, ConferenceHall>(db, tracker), IConferenceHallRepository
{
    public async Task<IEnumerable<ConferenceHall>> GetAvailableConferenceHalls(DateRange range, Capacity seats, CancellationToken ct = default) { var unavailable = DbContext.Set<BookingEntity>().Where(x => x.Status == BookingStatus.Reserved && x.Duration.Start < range.End && x.Duration.End > range.Start).Select(x => x.ConferenceHallId); var items = await DbSet.AsNoTracking().Where(x => !unavailable.Contains(x.Id) && x.Seats.Value >= seats.Value).OrderBy(x => x.Name).ToListAsync(ct); return items.Select(ToModel); }
    protected override IQueryable<ConferenceHallEntity> Ordered(IQueryable<ConferenceHallEntity> q) => q.OrderBy(x => x.Id); protected override Guid GetEntityId(ConferenceHallEntity x) => x.Id; protected override Guid GetModelId(ConferenceHall x) => x.Id;
    protected override ConferenceHallEntity ToEntity(ConferenceHall x) => new() { Id = x.Id, Name = x.Name, Seats = x.Seats, Price = x.Price, LastBookedOnUtc = x.LastBookedOnUtc, Amenities = x.Amenities.ToList() };
    protected override ConferenceHall ToModel(ConferenceHallEntity x) => ConferenceHall.Restore(x.Id, x.Name, x.Seats, x.Price, x.Amenities, x.LastBookedOnUtc);
    protected override void UpdateEntity(ConferenceHallEntity e, ConferenceHall m) { e.Name = m.Name; e.Seats = m.Seats; e.Price = m.Price; e.LastBookedOnUtc = m.LastBookedOnUtc; e.Amenities = m.Amenities.ToList(); }
}
