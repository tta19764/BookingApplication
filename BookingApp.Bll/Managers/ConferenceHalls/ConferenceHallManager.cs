using System.Globalization;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Managers.Validation;

namespace BookingApp.Bll.Managers.ConferenceHalls;

public sealed class ConferenceHallManager(IConferenceHallRepository hallRepository, IUnitOfWork unitOfWork)
    : IConferenceHallManager
{
    public async Task<Result<Guid>> AddHallAsync(string name, int capacity, decimal hourlyRate, string currencyCode,
        IReadOnlyCollection<Amenity> amenities, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidateHall(name, capacity, hourlyRate, amenities, currencyCode);
        var hall = new ConferenceHall(Guid.NewGuid(), new Name(name.Trim()), new Capacity(capacity),
            new Money(hourlyRate, Currency.FromCode(currencyCode.Trim().ToUpperInvariant())), amenities.Distinct().ToList());
        hallRepository.Add(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(hall.Id);
    }

    public async Task<Result<IReadOnlyCollection<HallModel>>> GetHallsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidatePage(page, pageSize);
        var halls = await hallRepository.GetListPaginatedAsync(page, pageSize, cancellationToken);
        return Result.Success<IReadOnlyCollection<HallModel>>(halls.Select(ToModel).ToList());
    }

    public async Task<Result<HallModel>> GetHallAsync(Guid hallId, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidateId(hallId, nameof(hallId));
        var hall = await hallRepository.GetByIdAsync(hallId, cancellationToken);
        return hall is null ? Result.Failure<HallModel>(ConferenceHallErrors.NotFound) : Result.Success(ToModel(hall));
    }

    public async Task<Result> UpdateHallAsync(Guid hallId, string name, int capacity, decimal hourlyRate,
        IReadOnlyCollection<Amenity> amenities, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidateId(hallId, nameof(hallId));
        ManagerInputValidator.ValidateHall(name, capacity, hourlyRate, amenities);
        var hall = await hallRepository.GetByIdAsync(hallId, cancellationToken);
        if (hall is null) return Result.Failure(ConferenceHallErrors.NotFound);
        hall.Name = new Name(name.Trim());
        hall.Seats = new Capacity(capacity);
        hall.Price = new Money(hourlyRate, Currency.Uah);
        hall.Amenities = amenities.Distinct().ToList();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RemoveHallAsync(Guid hallId, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidateId(hallId, nameof(hallId));
        var hall = await hallRepository.GetByIdAsync(hallId, cancellationToken);
        if (hall is null) return Result.Failure(ConferenceHallErrors.NotFound);
        hallRepository.Remove(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<HallModel>>> GetAvailableHallsAsync(DateOnly date, string startTime,
        string endTime, int capacity, CancellationToken cancellationToken)
    {
        ManagerInputValidator.ValidatePeriod(null, date, startTime, endTime, capacity);
        var duration = BookingPeriodFactory.Create(date, TimeOnly.ParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture));
        var halls = await hallRepository.GetAvailableConferenceHalls(duration, new Capacity(capacity), cancellationToken);
        return Result.Success(halls.Select(ToModel));
    }

    private static HallModel ToModel(ConferenceHall hall) => new(hall.Id, hall.Name.Value, hall.Seats.Value,
        hall.Price.Amount, hall.Price.Currency.Code, hall.Amenities.Select(amenity =>
        {
            var price = amenity.GetPrice(hall.Price.Currency);
            return new AmenityModel(amenity, amenity == Amenity.WiFi ? "Wi-Fi" : amenity.ToString(), price.Amount, price.Currency.Code);
        }).ToList());
}
