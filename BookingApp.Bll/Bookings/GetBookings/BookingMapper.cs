using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Common.Bookings;

namespace BookingApp.Bll.Bookings.GetBookings;

/// <summary>
/// Maps booking domain entities to booking read models.
/// </summary>
internal static class BookingMapper
{
    /// <summary>
    /// Converts a persisted booking into an API-safe response model.
    /// </summary>
    internal static BookingModel ToModel(Booking booking)
    {
        return new BookingModel(
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
