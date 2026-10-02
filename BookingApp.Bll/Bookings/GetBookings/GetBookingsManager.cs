using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;

namespace BookingApp.Bll.Bookings.GetBookings;

/// <summary>
/// Handles paginated booking list queries.
/// </summary>
public sealed class GetBookingsManager(IBookingRepository bookingRepository)
    : IRequestManager<GetBookingsRequest, Result<IReadOnlyCollection<BookingModel>>>
{
    public async Task<Result<IReadOnlyCollection<BookingModel>>> Handle(
        GetBookingsRequest request,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetListPaginatedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = bookings
            .Select(BookingMapper.ToModel)
            .ToList();

        return Result.Success<IReadOnlyCollection<BookingModel>>(response);
    }
}
