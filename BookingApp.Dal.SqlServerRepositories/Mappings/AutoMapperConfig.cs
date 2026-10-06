using System.Globalization;
using AutoMapper;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Dal.SqlServerRepositories.Entities;

namespace BookingApp.Dal.SqlServerRepositories.Mappings;

/// <summary>Maps DAL storage representations to provider-independent business models and back.</summary>
public sealed class AutoMapperConfig : Profile
{
    /// <summary>Defines bidirectional maps between DAL entities and Common business models.</summary>
    /// <remarks>Restores value objects and validates stored status and amenity values. Related bookings are intentionally not hydrated.</remarks>
    public AutoMapperConfig()
    {
        CreateMap<ConferenceHallEntity, ConferenceHall>()
            .ForMember(model => model.Name, options => options.MapFrom(entity => new Name(entity.Name)))
            .ForMember(model => model.Seats, options => options.MapFrom(entity => new Capacity(entity.Capacity)))
            .ForMember(model => model.Price, options => options.MapFrom(entity => new Money(entity.HourlyRate, Currency.FromCode(entity.Currency))))
            .ForMember(model => model.Amenities, options => options.MapFrom(entity => ParseAmenities(entity.Amenities)))
            .ForMember(model => model.Bookings, options => options.Ignore());

        CreateMap<ConferenceHall, ConferenceHallEntity>()
            .ForMember(entity => entity.Name, options => options.MapFrom(model => model.Name.Value))
            .ForMember(entity => entity.Capacity, options => options.MapFrom(model => model.Seats.Value))
            .ForMember(entity => entity.HourlyRate, options => options.MapFrom(model => model.Price.Amount))
            .ForMember(entity => entity.Currency, options => options.MapFrom(model => model.Price.Currency.Code))
            .ForMember(entity => entity.Amenities, options => options.MapFrom(model => string.Join(',', model.Amenities.Select(amenity => (int)amenity))));

        CreateMap<BookingEntity, Booking>()
            .ForMember(model => model.Duration, options => options.MapFrom(entity => DateRange.Create(entity.Start, entity.End)))
            .ForMember(model => model.PriceForPeriod, options => options.MapFrom(entity => new Money(entity.PriceForPeriodAmount, Currency.FromCode(entity.PriceForPeriodCurrency))))
            .ForMember(model => model.AmenitiesUpCharge, options => options.MapFrom(entity => new Money(entity.AmenitiesUpChargeAmount, Currency.FromCode(entity.AmenitiesUpChargeCurrency))))
            .ForMember(model => model.TotalPrice, options => options.MapFrom(entity => new Money(entity.TotalPriceAmount, Currency.FromCode(entity.TotalPriceCurrency))))
            .ForMember(model => model.Status, options => options.MapFrom(entity => ParseStatus(entity.Status)))
            .ForMember(model => model.ConferenceHall, options => options.Ignore())
            .ForMember(model => model.User, options => options.Ignore());

        CreateMap<Booking, BookingEntity>()
            .ForMember(entity => entity.Start, options => options.MapFrom(model => model.Duration.Start))
            .ForMember(entity => entity.End, options => options.MapFrom(model => model.Duration.End))
            .ForMember(entity => entity.PriceForPeriodCurrency, options => options.MapFrom(model => model.PriceForPeriod.Currency.Code))
            .ForMember(entity => entity.AmenitiesUpChargeCurrency, options => options.MapFrom(model => model.AmenitiesUpCharge.Currency.Code))
            .ForMember(entity => entity.TotalPriceCurrency, options => options.MapFrom(model => model.TotalPrice.Currency.Code))
            .ForMember(entity => entity.Status, options => options.MapFrom(model => model.Status.ToString()));

        CreateMap<UserEntity, User>()
            .ForMember(model => model.FirstName, options => options.MapFrom(entity => new FirstName(entity.FirstName)))
            .ForMember(model => model.LastName, options => options.MapFrom(entity => new LastName(entity.LastName)))
            .ForMember(model => model.Email, options => options.MapFrom(entity => new Email(entity.Email)))
            .ForMember(model => model.Bookings, options => options.Ignore());

        CreateMap<User, UserEntity>()
            .ForMember(entity => entity.FirstName, options => options.MapFrom(model => model.FirstName.Value))
            .ForMember(entity => entity.LastName, options => options.MapFrom(model => model.LastName.Value))
            .ForMember(entity => entity.Email, options => options.MapFrom(model => model.Email.Value));

        CreateMap<RoleEntity, Role>().ForMember(model => model.Users, options => options.Ignore());
        CreateMap<Role, RoleEntity>();
        CreateMap<PermissionEntity, Permission>().ReverseMap();
        CreateMap<RolePermission, RolePermissionEntity>().ReverseMap();
    }

    private static List<Amenity> ParseAmenities(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(item =>
    {
        var amenity = (Amenity)int.Parse(item, CultureInfo.InvariantCulture);
        return Enum.IsDefined(amenity) ? amenity : throw new InvalidDataException("Unknown stored amenity");
    }).ToList();

    private static BookingStatus ParseStatus(string value) =>
        Enum.TryParse<BookingStatus>(value, out var status) && Enum.IsDefined(status)
            ? status : throw new InvalidDataException("Unknown stored booking status");
}
