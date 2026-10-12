using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared;
using BookingApp.Bll.Common.Shared.Models;

namespace BookingApp.Bll.Common.ConferenceHalls;

/// <summary>
/// Defines conference hall operations exposed by the business logic layer.
/// </summary>
public interface IConferenceHallManager
{
    /// <summary>Validates and creates a conference hall.</summary>
    /// <param name="model">Validated input for the requested operation.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>The new hall identifier, or a validation failure.</returns>
    Task<Result<Guid>> AddHallAsync(
        CreateHallModel model,
        CancellationToken cancellationToken);

    /// <summary>Gets a page of conference halls after validating pagination.</summary>
    /// <param name="pagination">One-based page number and maximum page size.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>A page of hall details, or a validation failure.</returns>
    Task<Result<IReadOnlyCollection<HallModel>>> GetHallsAsync(
        PaginationModel pagination,
        CancellationToken cancellationToken);

    /// <summary>Gets a conference hall by its identifier.</summary>
    /// <param name="model">Validated input for the requested operation.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>Hall details, or a not-found failure.</returns>
    Task<Result<HallModel>> GetHallAsync(HallReferenceModel model, CancellationToken cancellationToken);

    /// <summary>Validates and updates the editable conference hall details.</summary>
    /// <param name="model">Validated input for the requested operation.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>Success, or a validation or not-found failure.</returns>
    Task<Result> UpdateHallAsync(
        UpdateHallModel model,
        CancellationToken cancellationToken);

    /// <summary>Removes a hall if no reserved bookings prevent deletion.</summary>
    /// <param name="model">Validated input for the requested operation.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>Success, or a not-found or reservation-conflict failure.</returns>
    Task<Result> RemoveHallAsync(HallReferenceModel model, CancellationToken cancellationToken);

    /// <summary>Finds halls with enough seats and no overlapping reservations.</summary>
    /// <param name="model">Validated input for the requested operation.</param>
    /// <param name="cancellationToken">Cancels validation and asynchronous resource access.</param>
    /// <returns>Matching hall details, or a validation failure.</returns>
    Task<Result<IEnumerable<HallModel>>> GetAvailableHallsAsync(
        FindAvailableHallsModel model,
        CancellationToken cancellationToken);
}
