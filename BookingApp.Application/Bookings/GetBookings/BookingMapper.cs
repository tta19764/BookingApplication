using BookingApp.Domain.Bookings;

namespace BookingApp.Application.Bookings.GetBookings;

/// <summary>
/// Maps booking domain entities to booking read models.
/// </summary>
internal static class BookingMapper
{
    /// <summary>
    /// Converts a persisted booking into an API-safe response model.
    /// </summary>
    internal static BookingResponse ToResponse(Booking booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.ConferenceHallId,
            booking.UserId,
            booking.Duration.Start,
            booking.Duration.End,
            booking.Status.ToString(),
            booking.PriceForPeriod.Amount,
            booking.AmenitiesUpCharge.Amount,
            booking.TotalPrice.Amount,
            booking.TotalPrice.Currency.Code);
    }
}
