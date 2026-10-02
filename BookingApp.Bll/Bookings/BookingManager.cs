using System.Globalization;
using BookingApp.Bll.Abstractions.Clock;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Validation;

namespace BookingApp.Bll.Bookings;

public sealed class BookingManager(IConferenceHallRepository hallRepository, IBookingRepository bookingRepository,
    IPricingManager pricingManager, IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork) : IBookingManager
{
    public async Task<Result<IReadOnlyCollection<BookingModel>>> GetBookingsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidatePage(page, pageSize);
        var bookings = await bookingRepository.GetListPaginatedAsync(page, pageSize, cancellationToken);
        return Result.Success<IReadOnlyCollection<BookingModel>>(bookings.Select(ToModel).ToList());
    }

    public async Task<Result<BookingConfirmationModel>> AddBookingAsync(Guid hallId, Guid userId, DateOnly date,
        string startTime, string endTime, IReadOnlyCollection<Amenity> amenities, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidatePeriod(hallId, date, startTime, endTime);
        if (userId == Guid.Empty) throw new BookingApp.Bll.Common.Shared.Exceptions.ValidationException(
            [new(nameof(userId), "User ID is required.")]);
        var hall = await hallRepository.GetByIdAsync(hallId, cancellationToken);
        if (hall is null) return Result.Failure<BookingConfirmationModel>(ConferenceHallErrors.NotFound);

        var duration = BookingPeriodFactory.Create(date,
            TimeOnly.ParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture));
        if (duration.Start <= dateTimeProvider.UtcNow) return Result.Failure<BookingConfirmationModel>(BookingErrors.StartsInPast);
        if (await bookingRepository.HasOverlap(hall.Id, duration, cancellationToken))
            return Result.Failure<BookingConfirmationModel>(BookingErrors.Overlap);

        try
        {
            var pricing = pricingManager.CalculatePrice(hall, duration, amenities.Distinct());
            var booking = new Booking(Guid.NewGuid())
            {
                ConferenceHallId = hall.Id,
                UserId = userId,
                Duration = duration,
                PriceForPeriod = pricing.PriceForPeriod,
                AmenitiesUpCharge = pricing.AmenitiesUpCharge,
                TotalPrice = pricing.TotalPrice,
                Status = BookingStatus.Reserved,
                CreatedOnUtc = dateTimeProvider.UtcNow
            };
            hall.LastBookedOnUtc = dateTimeProvider.UtcNow;
            bookingRepository.Add(booking);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(new BookingConfirmationModel(booking.Id, booking.ConferenceHallId,
                booking.Duration.Start, booking.Duration.End, booking.PriceForPeriod.Amount,
                booking.AmenitiesUpCharge.Amount, booking.TotalPrice.Amount, booking.TotalPrice.Currency.Code));
        }
        catch (ArgumentException)
        {
            return Result.Failure<BookingConfirmationModel>(new Error("Booking.UnsupportedAmenity",
                "The hall does not support one or more selected amenities"));
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure<BookingConfirmationModel>(new Error("Booking.InvalidPeriod", exception.Message));
        }
    }

    private static BookingModel ToModel(Booking booking) => new(booking.Id, booking.ConferenceHallId, booking.UserId,
        booking.Duration.Start, booking.Duration.End, booking.Status.ToString(), booking.PriceForPeriod.Amount,
        booking.AmenitiesUpCharge.Amount, booking.TotalPrice.Amount, booking.TotalPrice.Currency.Code);
}
