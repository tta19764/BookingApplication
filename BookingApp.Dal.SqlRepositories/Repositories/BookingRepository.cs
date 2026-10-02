using BookingApp.Bll.Common.Bookings;
using BookingApp.Dal.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
namespace BookingApp.Dal.SqlRepositories.Repositories;

public sealed class BookingRepository(ApplicationDbContext db, EntityChangeTracker tracker) : Repository<BookingEntity, Booking>(db, tracker), IBookingRepository
{
    public Task<bool> HasOverlap(Guid id, DateRange r, CancellationToken ct = default) => DbSet.AnyAsync(x => x.ConferenceHallId == id && x.Duration.Start < r.End && x.Duration.End > r.Start, ct);
    public async IAsyncEnumerable<IReadOnlyCollection<Booking>> List(int size, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default) { for (var p = 0; ; p++) { var x = await DbSet.AsNoTracking().OrderBy(x => x.Id).Skip(p * size).Take(size).ToListAsync(ct); if (x.Count == 0) yield break; yield return x.Select(ToModel).ToList(); } }
    public async Task<IReadOnlyCollection<Booking>> GetReservedBookingsDueForCompletion(DateTime now, int size, CancellationToken ct = default) { var x = await DbSet.Where(x => x.Status == BookingStatus.Reserved && x.Duration.End <= now).OrderBy(x => x.Duration.End).Take(size).ToListAsync(ct); return x.Select(Track).ToList(); }
    protected override IQueryable<BookingEntity> Ordered(IQueryable<BookingEntity> q) => q.OrderBy(x => x.Id); protected override Guid GetEntityId(BookingEntity x) => x.Id; protected override Guid GetModelId(Booking x) => x.Id;
    protected override BookingEntity ToEntity(Booking x) => new() { Id = x.Id, ConferenceHallId = x.ConferenceHallId, UserId = x.UserId, Duration = x.Duration, PriceForPeriod = x.PriceForPeriod, AmenitiesUpCharge = x.AmenitiesUpCharge, TotalPrice = x.TotalPrice, Status = x.Status, CreatedOnUtc = x.CreatedOnUtc, RejectedOnUtc = x.RejectedOnUtc, CompletedOnUtc = x.CompletedOnUtc, CancelledOnUtc = x.CancelledOnUtc };
    protected override Booking ToModel(BookingEntity x) => Booking.Restore(x.Id, x.ConferenceHallId, x.UserId, x.Duration, x.PriceForPeriod, x.AmenitiesUpCharge, x.TotalPrice, x.Status, x.CreatedOnUtc, x.RejectedOnUtc, x.CompletedOnUtc, x.CancelledOnUtc);
    protected override void UpdateEntity(BookingEntity e, Booking m) { e.Status = m.Status; e.RejectedOnUtc = m.RejectedOnUtc; e.CompletedOnUtc = m.CompletedOnUtc; e.CancelledOnUtc = m.CancelledOnUtc; }
}
