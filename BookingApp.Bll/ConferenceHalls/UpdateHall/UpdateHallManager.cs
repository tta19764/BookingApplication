using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.ConferenceHalls.UpdateHall;

/// <summary>
/// Updates an existing hall or returns a not-found result when the hall does not exist.
/// </summary>
public class UpdateHallManager(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork) : IRequestManager<UpdateHallRequest, Result>
{
    public async Task<Result> Handle(UpdateHallRequest request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        if (hall is null)
        {
            return Result.Failure(ConferenceHallErrors.NotFound);
        }

        hall.Update(
            new Name(request.Name.Trim()),
            new Capacity(request.Capacity),
            new Money(request.HourlyRate, Currency.Uah),
            request.Amenities);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
