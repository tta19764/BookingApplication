using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.ConferenceHalls;
using BookingApp.Domain.Shared;

namespace BookingApp.Application.ConferenceHalls.AddHall;

/// <summary>
/// Creates a conference hall and persists it through the hall repository.
/// </summary>
public class AddHallCommandHandler(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddHallCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddHallCommand request, CancellationToken cancellationToken)
    {
        var hall = new ConferenceHall(
            Guid.NewGuid(),
            new Name(request.Name.Trim()),
            new Capacity(request.Capacity),
            new Money(request.HourlyRate, Currency.Uah),
            request.Amenities.Distinct().ToList());

        hallRepository.Add(hall);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(hall.Id);
    }
}