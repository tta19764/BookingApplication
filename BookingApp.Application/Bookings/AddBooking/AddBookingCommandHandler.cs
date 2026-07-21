using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.Bookings.AddBooking;

/// <summary>
/// Creates a booking after validating hall existence, amenity support, and schedule availability.
/// </summary>
public class AddBookingCommandHandler(
    IConferenceHallRepository hallRepository,
    IBookingRepository bookingRepository,
    PricingService pricingService,
    IUnitOfWork unitOfWork) : ICommandHandler<AddBookingCommand, BookingConfirmationResponse>
{
    public async Task<Result<BookingConfirmationResponse>> Handle(
        AddBookingCommand request,
        CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetById(request.HallId, cancellationToken);

        if (hall is null)
        {
            return Result.Failure<BookingConfirmationResponse>(ConferenceHallErrors.NotFound);
        }

        var duration = DateRange.Create(request.Start, request.End);

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
                DateTime.UtcNow,
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
            return Result.Failure<BookingConfirmationResponse>(
                new Error("Booking.UnsupportedAmenity", "The hall does not support one or more selected amenities"));
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure<BookingConfirmationResponse>(
                new Error("Booking.InvalidPeriod", exception.Message));
        }
    }
}