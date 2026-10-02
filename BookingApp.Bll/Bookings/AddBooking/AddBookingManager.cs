using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Clock;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using System.Globalization;

namespace BookingApp.Bll.Bookings.AddBooking;

/// <summary>
/// Creates a booking after validating hall existence, amenity support, and schedule availability.
/// </summary>
public class AddBookingManager(
    IConferenceHallRepository hallRepository,
    IBookingRepository bookingRepository,
    PricingService pricingService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : IRequestManager<AddBookingRequest, Result<BookingConfirmationModel>>
{
    public async Task<Result<BookingConfirmationModel>> Handle(
        AddBookingRequest request,
        CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        if (hall is null)
        {
            return Result.Failure<BookingConfirmationModel>(ConferenceHallErrors.NotFound);
        }

        var duration = DateRange.Create(
            request.Date,
            TimeOnly.ParseExact(request.StartTime, "HH:mm", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact(request.EndTime, "HH:mm", CultureInfo.InvariantCulture));

        if (duration.Start <= dateTimeProvider.UtcNow)
        {
            return Result.Failure<BookingConfirmationModel>(BookingErrors.StartsInPast);
        }

        // Prevent double-booking before creating the reservation aggregate.
        if (await bookingRepository.HasOverlap(hall.Id, duration, cancellationToken))
        {
            return Result.Failure<BookingConfirmationModel>(BookingErrors.Overlap);
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

            return Result.Success(new BookingConfirmationModel(
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
            return Result.Failure<BookingConfirmationModel>(
                new Error("Booking.UnsupportedAmenity", "The hall does not support one or more selected amenities"));
        }
        catch (InvalidOperationException exception)
        {
            // Pricing rejects periods outside allowed business hours.
            return Result.Failure<BookingConfirmationModel>(
                new Error("Booking.InvalidPeriod", exception.Message));
        }
    }
}
