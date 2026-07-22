using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.GetHalls;

/// <summary>
/// Handles paginated conference hall list queries.
/// </summary>
public sealed class GetHallsQueryHandler(IConferenceHallRepository hallRepository)
    : IQueryHandler<GetHallsQuery, IReadOnlyCollection<HallResponse>>
{
    public async Task<Result<IReadOnlyCollection<HallResponse>>> Handle(
        GetHallsQuery request,
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
