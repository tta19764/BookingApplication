namespace BookingApp.Services.Web.Dtos;

public sealed record BookingDto(
    Guid Id,
    Guid HallId,
    Guid UserId,
    DateTime Start,
    DateTime End,
    string Status,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency);

public sealed record BookingConfirmationDto(
    Guid BookingId,
    Guid HallId,
    DateTime Start,
    DateTime End,
    decimal PriceForPeriod,
    decimal AmenitiesUpCharge,
    decimal TotalPrice,
    string Currency);
