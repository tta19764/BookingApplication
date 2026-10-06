using AutoMapper;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Reports.Models;
using BookingApp.Services.Web.Dtos;

namespace BookingApp.Services.Web.Mappings;

public sealed class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<AmenityModel, AmenityDto>();
        CreateMap<HallModel, ConferenceHallDto>();
        CreateMap<BookingModel, BookingDto>();
        CreateMap<BookingConfirmationModel, BookingConfirmationDto>();
        CreateMap<HallBookingSummaryModel, HallBookingSummaryDto>();
        CreateMap<BookingSummaryModel, BookingSummaryDto>();
    }
}
