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
    public async Task<Result<BookingSummaryResponse>> Handle(
        GetBookingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.List(cancellationToken);

        var hallSummaries = bookings
            .GroupBy(booking => booking.ConferenceHallId)
            .Select(group => new HallBookingSummaryResponse(
                group.Key,
                group.Count(),
                group.Sum(booking => booking.TotalPrice.Amount)))
            .OrderByDescending(summary => summary.Revenue)
            .ToList();

        var response = new BookingSummaryResponse(
            bookings.Count,
            bookings.Sum(booking => booking.TotalPrice.Amount),
            Currency.Uah.Code,
            hallSummaries);

        return Result.Success(response);
    }
}
