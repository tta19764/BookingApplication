using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Reports;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.Reports.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Managers.Reports;

public sealed class ReportManager(IBookingRepository bookingRepository) : IReportManager
{
    private const int PageSize = 500;

    public async Task<Result<BookingSummaryModel>> GetBookingSummaryAsync(CancellationToken cancellationToken)
    {
        var totalBookings = 0;
        var totalRevenue = 0m;
        var summaries = new Dictionary<Guid, HallBookingSummaryModel>();
        await foreach (var bookings in bookingRepository.ListAsync(PageSize, cancellationToken))
        {
            totalBookings += bookings.Count;
            totalRevenue += bookings.Sum(x => x.TotalPrice.Amount);
            foreach (var group in bookings.GroupBy(x => x.ConferenceHallId))
            {
                var count = group.Count();
                var revenue = group.Sum(x => x.TotalPrice.Amount);
                summaries[group.Key] = summaries.TryGetValue(group.Key, out var current)
                    ? current with { BookingCount = current.BookingCount + count, Revenue = current.Revenue + revenue }
                    : new HallBookingSummaryModel(group.Key, count, revenue);
            }
        }

        return Result.Success(new BookingSummaryModel(totalBookings, totalRevenue, Currency.Uah.Code,
            summaries.Values.OrderByDescending(x => x.Revenue).ToList()));
    }
}
