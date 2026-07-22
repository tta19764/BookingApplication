using BookingApp.Application.Abstractions.Messaging;

namespace BookingApp.Application.Bookings.GetBookings;

/// <summary>
/// Query for reading one page of bookings.
/// </summary>
public sealed record GetBookingsQuery(int Page, int PageSize) : IQuery<IReadOnlyCollection<BookingResponse>>;
