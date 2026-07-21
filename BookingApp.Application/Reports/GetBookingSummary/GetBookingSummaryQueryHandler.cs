using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.Shared;

namespace BookingApp.Application.Reports.GetBookingSummary;

/// <summary>
/// Builds a booking summary report grouped by conference hall.
/// </summary>
public class GetBookingSummaryQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetBookingSummaryQuery, BookingSummaryResponse>
{
    private const int PageSize = 500;

    public async Task<Result<BookingSummaryResponse>> Handle(
        GetBookingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var totalBookings = 0;
        var totalRevenue = 0m;
        var hallSummaries = new Dictionary<Guid, HallBookingSummaryResponse>();

        await foreach (var bookings in bookingRepository.List(PageSize, cancellationToken))
        {
            totalBookings += bookings.Count;
            totalRevenue += bookings.Sum(booking => booking.TotalPrice.Amount);

            // Group each page and merge it into the running summary to avoid keeping all bookings in memory.
            foreach (var group in bookings.GroupBy(booking => booking.ConferenceHallId))
            {
                var bookingCount = group.Count();
                var revenue = group.Sum(booking => booking.TotalPrice.Amount);

                if (hallSummaries.TryGetValue(group.Key, out var existingSummary))
                {
                    hallSummaries[group.Key] = existingSummary with
                    {
                        BookingCount = existingSummary.BookingCount + bookingCount,
                        Revenue = existingSummary.Revenue + revenue
                    };

                    continue;
                }

                hallSummaries[group.Key] = new HallBookingSummaryResponse(
                    group.Key,
                    bookingCount,
                    revenue);
            }
        }

        var response = new BookingSummaryResponse(
            totalBookings,
            totalRevenue,
            Currency.Uah.Code,
            hallSummaries.Values
                .OrderByDescending(summary => summary.Revenue)
                .ToList());

        return Result.Success(response);
    }
}
