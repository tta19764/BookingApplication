using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;

namespace BookingApp.Bll.ConferenceHalls.GetHalls;

/// <summary>
/// Handles paginated conference hall list queries.
/// </summary>
public sealed class GetHallsManager(IConferenceHallRepository hallRepository)
    : IRequestManager<GetHallsRequest, Result<IReadOnlyCollection<HallResponse>>>
{
    public async Task<Result<IReadOnlyCollection<HallResponse>>> Handle(
        GetHallsRequest request,
        CancellationToken cancellationToken)
    {
        var halls = await hallRepository.GetListPaginatedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = halls
            .Select(HallMapper.ToResponse)
            .ToList();

        return Result.Success<IReadOnlyCollection<HallResponse>>(response);
    }
}
