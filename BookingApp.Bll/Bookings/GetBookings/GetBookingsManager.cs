using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;

namespace BookingApp.Bll.Bookings.GetBookings;

/// <summary>
/// Handles paginated booking list queries.
/// </summary>
public sealed class GetBookingsManager(IBookingRepository bookingRepository)
    : IRequestManager<GetBookingsRequest, Result<IReadOnlyCollection<BookingResponse>>>
{
    public async Task<Result<IReadOnlyCollection<BookingResponse>>> Handle(
        GetBookingsRequest request,
        CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetListPaginatedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = bookings
            .Select(BookingMapper.ToResponse)
            .ToList();

        return Result.Success<IReadOnlyCollection<BookingResponse>>(response);
    }
}
