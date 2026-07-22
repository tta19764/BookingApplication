using BookingApp.Application.Abstractions.Messaging;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Domain.Abstractions;
using BookingApp.Domain.Bookings;
using BookingApp.Domain.ConferenceHalls;
using System.Globalization;

namespace BookingApp.Application.ConferenceHalls.GetAvailableHalls;

/// <summary>
/// Finds available halls and maps them to hall response models.
/// </summary>
public class GetAvailableHallsQueryHandler(IConferenceHallRepository hallRepository)
    : IQueryHandler<GetAvailableHallsQuery, IEnumerable<HallResponse>>
{
    public async Task<Result<IEnumerable<HallResponse>>> Handle(
        GetAvailableHallsQuery request,
        CancellationToken cancellationToken)
    {
        var duration = DateRange.Create(
            request.Date,
            TimeOnly.ParseExact(request.StartTime, "HH:mm", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact(request.EndTime, "HH:mm", CultureInfo.InvariantCulture));

        // Availability is delegated to the repository because overlap checks depend on stored bookings.
        var halls = await hallRepository.GetAvailableConferenceHalls(
            duration,
            new Capacity(request.Capacity),
            cancellationToken);

        return Result.Success(halls.Select(HallMapper.ToResponse));
    }
}
