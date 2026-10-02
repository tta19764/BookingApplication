using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Bll.Common.ConferenceHalls;

/// <summary>
/// Defines conference hall operations exposed by the business logic layer.
/// </summary>
public interface IConferenceHallManager
{
    Task<Result<Guid>> AddHallAsync(
        string name,
        int capacity,
        decimal hourlyRate,
        string currencyCode,
        IReadOnlyCollection<Amenity> amenities,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<HallModel>>> GetHallsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<HallModel>> GetHallAsync(Guid hallId, CancellationToken cancellationToken);

    Task<Result> UpdateHallAsync(
        Guid hallId,
        string name,
        int capacity,
        decimal hourlyRate,
        IReadOnlyCollection<Amenity> amenities,
        CancellationToken cancellationToken);

    Task<Result> RemoveHallAsync(Guid hallId, CancellationToken cancellationToken);

    Task<Result<IEnumerable<HallModel>>> GetAvailableHallsAsync(
        DateOnly date,
        string startTime,
        string endTime,
        int capacity,
        CancellationToken cancellationToken);
}
