using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.ConferenceHalls.GetHalls;

/// <summary>
/// Handles paginated conference hall list queries.
/// </summary>
public sealed class GetHallsManager(IConferenceHallRepository hallRepository)
    : IRequestManager<GetHallsRequest, Result<IReadOnlyCollection<HallModel>>>
{
    public async Task<Result<IReadOnlyCollection<HallModel>>> Handle(
        GetHallsRequest request,
        CancellationToken cancellationToken)
    {
        var halls = await hallRepository.GetListPaginatedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = halls
            .Select(HallMapper.ToModel)
            .ToList();

        return Result.Success<IReadOnlyCollection<HallModel>>(response);
    }
}
