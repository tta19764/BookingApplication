using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.ConferenceHalls.GetHall;

/// <summary>
/// Reads a single hall and maps it to the hall response model.
/// </summary>
public class GetHallManager(IConferenceHallRepository hallRepository)
    : IRequestManager<GetHallRequest, Result<HallModel>>
{
    public async Task<Result<HallModel>> Handle(GetHallRequest request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        return hall is null
            ? Result.Failure<HallModel>(ConferenceHallErrors.NotFound)
            : Result.Success(HallMapper.ToModel(hall));
    }
}
