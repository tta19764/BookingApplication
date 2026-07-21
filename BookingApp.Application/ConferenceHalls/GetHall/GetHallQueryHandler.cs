using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.ConferenceHalls.GetHall;

public class GetHallQueryHandler : IQueryHandler<GetHallQuery, HallResponse>
{
    public Task<Result<HallResponse>> Handle(GetHallQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}