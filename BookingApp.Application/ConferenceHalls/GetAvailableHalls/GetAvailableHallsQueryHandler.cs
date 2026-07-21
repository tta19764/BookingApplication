using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.ConferenceHalls.GetAvailableHalls;

public class GetAvailableHallsQueryHandler : IQueryHandler<GetAvailableHallsQuery, IEnumerable<HallResponse>>
{
    public Task<Result<IEnumerable<HallResponse>>> Handle(GetAvailableHallsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}