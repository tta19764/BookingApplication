using AutoMapper;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Users;
using BookingApp.Dal.SqlRepositories.Entities;

namespace BookingApp.Dal.SqlRepositories.Mappings;

public sealed class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<ConferenceHall, ConferenceHallEntity>()
            .ForMember(destination => destination.Bookings, options => options.Ignore());
        CreateMap<ConferenceHallEntity, ConferenceHall>()
            .ForMember(destination => destination.Bookings, options => options.Ignore());

        CreateMap<Booking, BookingEntity>()
            .ForMember(destination => destination.ConferenceHall, options => options.Ignore())
            .ForMember(destination => destination.User, options => options.Ignore());
        CreateMap<BookingEntity, Booking>()
            .ForMember(destination => destination.ConferenceHall, options => options.Ignore())
            .ForMember(destination => destination.User, options => options.Ignore());

        CreateMap<User, UserEntity>()
            .ForMember(destination => destination.Bookings, options => options.Ignore());
        CreateMap<UserEntity, User>()
            .ForMember(destination => destination.Bookings, options => options.Ignore());
    }
}
