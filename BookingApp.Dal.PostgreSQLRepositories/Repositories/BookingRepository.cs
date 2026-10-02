using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class BookingRepository(ApplicationDbContext db, EntityChangeTracker tracker, IMapper mapper) : Repository<BookingEntity, Booking>(db, tracker, mapper), IBookingRepository
{
    public Task<bool> HasOverlap(Guid id, DateRange r, CancellationToken ct = default) => DbSet.AnyAsync(x => x.ConferenceHallId == id && x.Duration.Start < r.End && x.Duration.End > r.Start, ct);
    public async IAsyncEnumerable<IReadOnlyCollection<Booking>> List(int size, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default) { for (var p = 0; ; p++) { var x = await DbSet.AsNoTracking().OrderBy(x => x.Id).Skip(p * size).Take(size).ToListAsync(ct); if (x.Count == 0) yield break; yield return x.Select(ToModel).ToList(); } }
    public async Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletion(DateTime now, int size, CancellationToken ct = default) { var x = await DbSet.Where(x => x.Status == BookingStatus.Reserved && x.Duration.End <= now).OrderBy(x => x.Duration.End).Take(size).ToListAsync(ct); return x.Select(Track).ToList(); }
    protected override IQueryable<BookingEntity> Ordered(IQueryable<BookingEntity> q) => q.OrderBy(x => x.Id); protected override Guid GetEntityId(BookingEntity x) => x.Id; protected override Guid GetModelId(Booking x) => x.Id;
}
