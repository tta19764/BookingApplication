using BookingApp.Application.Abstractions.Clock;
using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using System.Globalization;

namespace BookingApp.Application.Bookings.AddBooking;

/// <summary>
/// Creates a booking after validating hall existence, amenity support, and schedule availability.
/// </summary>
public class AddBookingCommandHandler(
    IConferenceHallRepository hallRepository,
    IBookingRepository bookingRepository,
    PricingService pricingService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<AddBookingCommand, BookingConfirmationResponse>
{
    public async Task<Result<BookingConfirmationResponse>> Handle(
        AddBookingCommand request,
        CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        if (hall is null)
        {
            return Result.Failure<BookingConfirmationResponse>(ConferenceHallErrors.NotFound);
        }

        var duration = DateRange.Create(
            request.Date,
            TimeOnly.ParseExact(request.StartTime, "HH:mm", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact(request.EndTime, "HH:mm", CultureInfo.InvariantCulture));

        if (duration.Start <= dateTimeProvider.UtcNow)
        {
            return Result.Failure<BookingConfirmationResponse>(BookingErrors.StartsInPast);
        }

        // Prevent double-booking before creating the reservation aggregate.
        if (await bookingRepository.HasOverlap(hall.Id, duration, cancellationToken))
        {
            return Result.Failure<BookingConfirmationResponse>(BookingErrors.Overlap);
        }

        try
        {
            var booking = Booking.Reserve(
                hall,
                request.Amenities.Distinct(),
                request.UserId,
                duration,
                dateTimeProvider.UtcNow,
                pricingService);

            bookingRepository.Add(booking);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new BookingConfirmationResponse(
                booking.Id,
                booking.ConferenceHallId,
                booking.Duration.Start,
                booking.Duration.End,
                booking.PriceForPeriod.Amount,
                booking.AmenitiesUpCharge.Amount,
                booking.TotalPrice.Amount,
                booking.TotalPrice.Currency.Code));
        }
        catch (ArgumentException)
        {
            // Domain amenity failures are returned as application results instead of leaking exceptions to API callers.
            return Result.Failure<BookingConfirmationResponse>(
                new Error("Booking.UnsupportedAmenity", "The hall does not support one or more selected amenities"));
        }
        catch (InvalidOperationException exception)
        {
            // Pricing rejects periods outside allowed business hours.
            return Result.Failure<BookingConfirmationResponse>(
                new Error("Booking.InvalidPeriod", exception.Message));
        }
    }
}
