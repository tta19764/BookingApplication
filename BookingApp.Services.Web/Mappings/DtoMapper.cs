using BookingApp.Bll.Common.Models;
using BookingApp.Services.Web.Dtos;

namespace BookingApp.Services.Web.Mappings;

public static class DtoMapper
{
    public static ConferenceHallDto ToDto(HallModel model) => new(
        model.Id,
        model.Name,
        model.Capacity,
        model.HourlyRate,
        model.Currency,
        model.Amenities.Select(amenity => new AmenityDto(
            amenity.Type,
            amenity.Name,
            amenity.Price,
            amenity.Currency)).ToList());

    public static BookingDto ToDto(BookingModel model) => new(
        model.Id, model.HallId, model.UserId, model.Start, model.End, model.Status,
        model.PriceForPeriod, model.AmenitiesUpCharge, model.TotalPrice, model.Currency);

    public static BookingConfirmationDto ToDto(BookingConfirmationModel model) => new(
        model.BookingId, model.HallId, model.Start, model.End, model.PriceForPeriod,
        model.AmenitiesUpCharge, model.TotalPrice, model.Currency);

    public static BookingSummaryDto ToDto(BookingSummaryModel model) => new(
        model.TotalBookings,
        model.TotalRevenue,
        model.Currency,
        model.Halls.Select(hall => new HallBookingSummaryDto(
            hall.HallId,
            hall.BookingCount,
            hall.Revenue)).ToList());
}
