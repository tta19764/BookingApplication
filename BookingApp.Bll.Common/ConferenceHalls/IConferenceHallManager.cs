using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Shared.Models;

namespace BookingApp.Bll.Common.ConferenceHalls;

/// <summary>
/// Defines conference hall operations exposed by the business logic layer.
/// </summary>
public interface IConferenceHallManager
{
    Task<Result<Guid>> AddHallAsync(
        CreateHallModel model,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<HallModel>>> GetHallsAsync(
        PaginationModel pagination,
        CancellationToken cancellationToken);

    Task<Result<HallModel>> GetHallAsync(HallReferenceModel model, CancellationToken cancellationToken);

    Task<Result> UpdateHallAsync(
        UpdateHallModel model,
        CancellationToken cancellationToken);

    Task<Result> RemoveHallAsync(HallReferenceModel model, CancellationToken cancellationToken);

    Task<Result<IEnumerable<HallModel>>> GetAvailableHallsAsync(
        FindAvailableHallsModel model,
        CancellationToken cancellationToken);
}
