using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.RemoveHall;

/// <summary>
/// Removes an existing hall or returns a not-found result when the hall does not exist.
/// </summary>
public class RemoveHallCommandHandler(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveHallCommand>
{
    public async Task<Result> Handle(RemoveHallCommand request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetById(request.HallId, cancellationToken);

        if (hall is null)
        {
            return Result.Failure(ConferenceHallErrors.NotFound);
        }

        hallRepository.Remove(hall);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}