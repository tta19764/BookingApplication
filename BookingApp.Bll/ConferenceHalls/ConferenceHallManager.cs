using BookingApp.Bll.Abstractions.Messaging;
using BookingApp.Bll.Common.Abstractions;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.Models;
using BookingApp.Bll.ConferenceHalls.AddHall;
using BookingApp.Bll.ConferenceHalls.GetAvailableHalls;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.ConferenceHalls.GetHalls;
using BookingApp.Bll.ConferenceHalls.RemoveHall;
using BookingApp.Bll.ConferenceHalls.UpdateHall;

namespace BookingApp.Bll.ConferenceHalls;

/// <summary>
/// Coordinates conference hall use cases through the BLL validation pipeline.
/// </summary>
internal sealed class ConferenceHallManager(IManagerDispatcher dispatcher) : IConferenceHallManager
{
    public Task<Result<Guid>> AddHallAsync(
        string name,
        int capacity,
        decimal hourlyRate,
        string currencyCode,
        IReadOnlyCollection<Amenity> amenities,
        CancellationToken cancellationToken) =>
        dispatcher.Send(new AddHallRequest(name, capacity, hourlyRate, currencyCode, amenities), cancellationToken);

    public Task<Result<IReadOnlyCollection<HallModel>>> GetHallsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        dispatcher.Send(new GetHallsRequest(page, pageSize), cancellationToken);

    public Task<Result<HallModel>> GetHallAsync(Guid hallId, CancellationToken cancellationToken) =>
        dispatcher.Send(new GetHallRequest(hallId), cancellationToken);

    public Task<Result> UpdateHallAsync(
        Guid hallId,
        string name,
        int capacity,
        decimal hourlyRate,
        IReadOnlyCollection<Amenity> amenities,
        CancellationToken cancellationToken) =>
        dispatcher.Send(new UpdateHallRequest(hallId, name, capacity, hourlyRate, amenities), cancellationToken);

    public Task<Result> RemoveHallAsync(Guid hallId, CancellationToken cancellationToken) =>
        dispatcher.Send(new RemoveHallRequest(hallId), cancellationToken);

    public Task<Result<IEnumerable<HallModel>>> GetAvailableHallsAsync(
        DateOnly date,
        string startTime,
        string endTime,
        int capacity,
        CancellationToken cancellationToken) =>
        dispatcher.Send(new GetAvailableHallsRequest(date, startTime, endTime, capacity), cancellationToken);
}
