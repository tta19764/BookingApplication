using AutoMapper;
using BookingApp.Bll.Managers.Bookings;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared.Models;
using BookingApp.Bll.Managers.ConferenceHalls.Validation;
using BookingApp.Bll.Managers.Shared.Validation;
using FluentValidation;

namespace BookingApp.Bll.Managers.ConferenceHalls;

public sealed class ConferenceHallManager(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IValidator<CreateHallModel> createHallValidator,
    IValidator<UpdateHallModel> updateHallValidator,
    IValidator<HallReferenceModel> hallReferenceValidator,
    IValidator<FindAvailableHallsModel> findAvailableHallsValidator,
    IValidator<PaginationModel> paginationValidator)
    : IConferenceHallManager
{
    public async Task<Result<Guid>> AddHallAsync(CreateHallModel model, CancellationToken cancellationToken)
    {
        await createHallValidator.ValidateForApplicationAsync(model, cancellationToken);
        var hall = new ConferenceHall(Guid.NewGuid(), new Name(model.Name.Trim()), new Capacity(model.Capacity),
            new Money(model.HourlyRate, Currency.FromCode(model.CurrencyCode.Trim().ToUpperInvariant())),
            model.Amenities.Distinct().ToList());
        hallRepository.Add(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(hall.Id);
    }

    public async Task<Result<IReadOnlyCollection<HallModel>>> GetHallsAsync(PaginationModel pagination,
        CancellationToken cancellationToken)
    {
        await paginationValidator.ValidateForApplicationAsync(pagination, cancellationToken);
        var halls = await hallRepository.GetListPaginatedAsync(pagination.Page, pagination.PageSize, cancellationToken);
        return Result.Success(mapper.Map<IReadOnlyCollection<HallModel>>(halls));
    }

    public async Task<Result<HallModel>> GetHallAsync(HallReferenceModel model, CancellationToken cancellationToken)
    {
        await hallReferenceValidator.ValidateForApplicationAsync(model, cancellationToken);
        var hall = await hallRepository.GetByIdAsync(model.HallId, cancellationToken);
        return hall is null
            ? Result.Failure<HallModel>(ConferenceHallErrors.NotFound)
            : Result.Success(mapper.Map<HallModel>(hall));
    }

    public async Task<Result> UpdateHallAsync(UpdateHallModel model, CancellationToken cancellationToken)
    {
        await updateHallValidator.ValidateForApplicationAsync(model, cancellationToken);
        var hall = await hallRepository.GetByIdAsync(model.HallId, cancellationToken);
        if (hall is null) return Result.Failure(ConferenceHallErrors.NotFound);
        hall.Name = new Name(model.Name.Trim());
        hall.Seats = new Capacity(model.Capacity);
        hall.Price = new Money(model.HourlyRate, Currency.Uah);
        hall.Amenities = model.Amenities.Distinct().ToList();
        hallRepository.Update(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RemoveHallAsync(HallReferenceModel model, CancellationToken cancellationToken)
    {
        await hallReferenceValidator.ValidateForApplicationAsync(model, cancellationToken);
        var hall = await hallRepository.GetByIdAsync(model.HallId, cancellationToken);
        if (hall is null) return Result.Failure(ConferenceHallErrors.NotFound);
        hallRepository.Remove(hall);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<HallModel>>> GetAvailableHallsAsync(FindAvailableHallsModel model,
        CancellationToken cancellationToken)
    {
        await findAvailableHallsValidator.ValidateForApplicationAsync(model, cancellationToken);
        var duration = BookingPeriodFactory.Create(model.Date, model.StartTime, model.EndTime);
        var halls = await hallRepository.GetAvailableConferenceHallsAsync(
            duration,
            new Capacity(model.Capacity),
            cancellationToken);
        return Result.Success(mapper.Map<IEnumerable<HallModel>>(halls));
    }
}
