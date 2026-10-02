using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;

namespace BookingApp.Bll.Bookings.GetBookings;

/// <summary>
/// Request for reading one page of bookings.
/// </summary>
public sealed record GetBookingsRequest(int Page, int PageSize) : IManagerRequest<Result<IReadOnlyCollection<BookingModel>>>;
