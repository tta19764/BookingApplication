using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;

namespace BookingApp.Application.ConferenceHalls.UpdateHall;

/// <summary>
/// Updates an existing hall or returns a not-found result when the hall does not exist.
/// </summary>
public class UpdateHallCommandHandler(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateHallCommand>
{
    public async Task<Result> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
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

        hallRepository.Update(hall);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
