using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.ConferenceHalls.RemoveHall;

/// <summary>
/// Removes an existing hall or returns a not-found result when the hall does not exist.
/// </summary>
public class RemoveHallManager(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork) : IRequestManager<RemoveHallRequest, Result>
{
    public async Task<Result> Handle(RemoveHallRequest request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        if (hall is null)
        {
            return Result.Failure(ConferenceHallErrors.NotFound);
        }

        hallRepository.Remove(hall);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
