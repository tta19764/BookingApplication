using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;

namespace BookingApp.Application.Bookings.GetBookings;

/// <summary>
/// Handles paginated booking list queries.
/// </summary>
public sealed class GetBookingsQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetBookingsQuery, IReadOnlyCollection<BookingResponse>>
{
    public async Task<Result<IReadOnlyCollection<BookingResponse>>> Handle(
        GetBookingsQuery request,
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
