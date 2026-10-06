using AutoMapper;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Events;
using BookingApp.Bll.Common.Bookings.Errors;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.Shared.Events;
using BookingApp.Bll.Common.Shared.Models;
using BookingApp.Bll.Managers.Shared.Validation;
using FluentValidation;
using BookingApp.Bll.Common.Users.Errors;

namespace BookingApp.Bll.Managers.Bookings;

public sealed class BookingManager(IConferenceHallRepository hallRepository, IBookingRepository bookingRepository,
    IPricingManager pricingManager, TimeProvider timeProvider,
    IDomainEventDispatcher domainEventDispatcher, IMapper mapper,
    IValidator<PaginationModel> paginationValidator,
    IValidator<CreateBookingModel> createBookingValidator) : IBookingManager
{
    public async Task<Result<IReadOnlyCollection<BookingModel>>> GetBookingsAsync(PaginationModel pagination,
        CancellationToken cancellationToken)
    {
        await paginationValidator.ValidateForApplicationAsync(pagination, cancellationToken);
        var bookings = await bookingRepository.GetListPaginatedAsync(pagination.Page, pagination.PageSize, cancellationToken);
        return Result.Success(mapper.Map<IReadOnlyCollection<BookingModel>>(bookings));
    }

    public async Task<Result<BookingConfirmationModel>> AddBookingAsync(CreateBookingModel model,
        CancellationToken cancellationToken)
    {
        await createBookingValidator.ValidateForApplicationAsync(model, cancellationToken);
        var hall = await hallRepository.GetByIdAsync(model.HallId, cancellationToken);
        if (hall is null) return Result.Failure<BookingConfirmationModel>(ConferenceHallErrors.NotFound);

        var duration = BookingPeriodFactory.Create(model.Date, model.StartTime, model.EndTime);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        if (duration.Start <= utcNow) return Result.Failure<BookingConfirmationModel>(BookingErrors.StartsInPast);

        if (await bookingRepository.HasOverlapAsync(hall.Id, duration, cancellationToken))
            return Result.Failure<BookingConfirmationModel>(BookingErrors.Overlap);

        PricingDetails pricing;
        try
        {
            pricing = pricingManager.CalculatePrice(hall, duration, model.Amenities.Distinct());
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

        var booking = new Booking(Guid.NewGuid())
        {
            ConferenceHallId = hall.Id,
            UserId = model.UserId,
            Duration = duration,
            PriceForPeriod = pricing.PriceForPeriod,
            AmenitiesUpCharge = pricing.AmenitiesUpCharge,
            TotalPrice = pricing.TotalPrice,
            Status = BookingStatus.Reserved,
            CreatedOnUtc = utcNow
        };
        var outcome = await bookingRepository.CreateReservationAsync(booking, cancellationToken);
        if (outcome != ReservationOutcome.Created)
        {
            var error = outcome switch
            {
                ReservationOutcome.Overlap => BookingErrors.Overlap,
                ReservationOutcome.HallNotFound => ConferenceHallErrors.NotFound,
                ReservationOutcome.UserNotFound => UserErrors.NotFound,
                _ => throw new InvalidOperationException("Unexpected reservation outcome")
            };
            return Result.Failure<BookingConfirmationModel>(error);
        }

        await domainEventDispatcher.DispatchAsync(
            new BookingCreatedDomainEvent(
                booking.Id,
                booking.ConferenceHallId,
                booking.UserId,
                booking.TotalPrice.Amount,
                booking.TotalPrice.Currency.Code,
                utcNow),
            cancellationToken);

        return Result.Success(new BookingConfirmationModel(booking.Id, booking.ConferenceHallId,
            booking.Duration.Start, booking.Duration.End, booking.PriceForPeriod.Amount,
            booking.AmenitiesUpCharge.Amount, booking.TotalPrice.Amount, booking.TotalPrice.Currency.Code));
    }
}
