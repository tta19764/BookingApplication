using BookingApp.Bll.Common.Models;
using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.ConferenceHalls;
using System.Globalization;

namespace BookingApp.Bll.ConferenceHalls.GetAvailableHalls;

/// <summary>
/// Finds available halls and maps them to hall response models.
/// </summary>
public class GetAvailableHallsManager(IConferenceHallRepository hallRepository)
    : IRequestManager<GetAvailableHallsRequest, Result<IEnumerable<HallModel>>>
{
    public async Task<Result<IEnumerable<HallModel>>> Handle(
        GetAvailableHallsRequest request,
        CancellationToken cancellationToken)
    {
        var duration = BookingApp.Bll.Bookings.BookingPeriodFactory.Create(
            request.Date,
            TimeOnly.ParseExact(request.StartTime, "HH:mm", CultureInfo.InvariantCulture),
            TimeOnly.ParseExact(request.EndTime, "HH:mm", CultureInfo.InvariantCulture));

        // Availability is delegated to the repository because overlap checks depend on stored bookings.
        var halls = await hallRepository.GetAvailableConferenceHalls(
            duration,
            new Capacity(request.Capacity),
            cancellationToken);

        return Result.Success(halls.Select(HallMapper.ToModel));
    }
}
