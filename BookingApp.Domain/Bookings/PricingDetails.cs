using BookingApp.Domain.Shared;

namespace BookingApp.Domain.Bookings;

public record PricingDetails(
    Money PriceForPeriod,
    Money AmenitiesUpCharge,
    Money TotalPrice);