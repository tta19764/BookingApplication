using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.ConferenceHalls.AddHall;

/// <summary>
/// Creates a conference hall and persists it through the hall repository.
/// </summary>
public class AddHallManager(
    IConferenceHallRepository hallRepository,
    IUnitOfWork unitOfWork) : IRequestManager<AddHallRequest, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddHallRequest request, CancellationToken cancellationToken)
    {
        var hall = new ConferenceHall(
            Guid.NewGuid(),
            new Name(request.Name.Trim()),
            new Capacity(request.Capacity),
            new Money(request.HourlyRate, Currency.FromCode(request.CurrencyCode.Trim().ToUpperInvariant())),
            request.Amenities.Distinct().ToList());

        hallRepository.Add(hall);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(hall.Id);
    }
}
