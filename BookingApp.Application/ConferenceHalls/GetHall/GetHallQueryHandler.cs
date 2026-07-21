using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.ConferenceHalls;

namespace BookingApp.Application.ConferenceHalls.GetHall;

/// <summary>
/// Reads a single hall and maps it to the hall response model.
/// </summary>
public class GetHallQueryHandler(IConferenceHallRepository hallRepository)
    : IQueryHandler<GetHallQuery, HallResponse>
{
    public async Task<Result<HallResponse>> Handle(GetHallQuery request, CancellationToken cancellationToken)
    {
        var hall = await hallRepository.GetById(request.HallId, cancellationToken);

        return hall is null
            ? Result.Failure<HallResponse>(ConferenceHallErrors.NotFound)
            : Result.Success(HallMapper.ToResponse(hall));
    }
}