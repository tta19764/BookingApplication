using AutoMapper;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;

namespace BookingApp.Bll.Mappings;

public sealed class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<Booking, BookingModel>()
            .ConvertUsing<BookingModelConverter>();

        CreateMap<ConferenceHall, HallModel>()
            .ConvertUsing<ConferenceHallModelConverter>();
    }
}

public sealed class BookingModelConverter : ITypeConverter<Booking, BookingModel>
{
    public BookingModel Convert(Booking source, BookingModel destination, ResolutionContext context) => new(
        source.Id,
        source.ConferenceHallId,
        source.UserId,
        source.Duration.Start,
        source.Duration.End,
        source.Status.ToString(),
        source.PriceForPeriod.Amount,
        source.AmenitiesUpCharge.Amount,
        source.TotalPrice.Amount,
        source.TotalPrice.Currency.Code);
}

public sealed class ConferenceHallModelConverter : ITypeConverter<ConferenceHall, HallModel>
{
    public HallModel Convert(ConferenceHall source, HallModel destination, ResolutionContext context)
    {
        var amenities = source.Amenities.Select(amenity =>
        {
            var price = amenity.GetPrice(source.Price.Currency);
            return new AmenityModel(
                amenity,
                amenity == Amenity.WiFi ? "Wi-Fi" : amenity.ToString(),
                price.Amount,
                price.Currency.Code);
        }).ToList();

        return new HallModel(
            source.Id,
            source.Name.Value,
            source.Seats.Value,
            source.Price.Amount,
            source.Price.Currency.Code,
            amenities);
    }
}
