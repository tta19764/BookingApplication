using Asp.Versioning;
using AutoMapper;
using BookingApp.Bll.Common.ConferenceHalls;
using BookingApp.Bll.Common.ConferenceHalls.Errors;
using BookingApp.Bll.Common.ConferenceHalls.Models;
using BookingApp.Bll.Common.Shared.Models;
using BookingApp.Services.Web.Configuration;
using BookingApp.Services.Web.Dtos;
using BookingApp.Services.Web.Dtos.Requests;
using BookingApp.Services.Web.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Services.Web.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/conference-halls")]
public sealed class ConferenceHallsController(IConferenceHallManager hallManager, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyCollection<ConferenceHallDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<IReadOnlyCollection<ConferenceHallDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetConferenceHalls(
        [FromQuery] GetConferenceHallsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await hallManager.GetHallsAsync(
            new PaginationModel(request.Page, request.PageSize),
            cancellationToken);
        var response = result.MapToApiResponse(mapper.Map<IReadOnlyCollection<ConferenceHallDto>>);

        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConferenceHall(
        [FromBody] CreateConferenceHallRequest request,
        CancellationToken cancellationToken)
    {
        var model = new CreateHallModel(
            request.Name,
            request.Capacity,
            request.HourlyRate,
            request.CurrencyCode,
            request.Amenities);
        var result = await hallManager.AddHallAsync(model, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(
                nameof(GetConferenceHall),
                new { version = BookingAppApiVersions.V1RouteValue, hallId = result.Value },
                result.MapToApiResponse())
            : BadRequest(result.MapToApiResponse());
    }

    [HttpGet("{hallId:guid}")]
    [ProducesResponseType<ApiResponse<ConferenceHallDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<ConferenceHallDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConferenceHall(Guid hallId, CancellationToken cancellationToken)
    {
        var result = await hallManager.GetHallAsync(new HallReferenceModel(hallId), cancellationToken);
        var response = result.MapToApiResponse(mapper.Map<ConferenceHallDto>);

        return result.IsSuccess ? Ok(response) : NotFound(response);
    }

    [HttpPut("{hallId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConferenceHall(
        Guid hallId,
        [FromBody] UpdateConferenceHallRequest request,
        CancellationToken cancellationToken)
    {
        var model = new UpdateHallModel(
            hallId,
            request.Name,
            request.Capacity,
            request.HourlyRate,
            request.Amenities);
        var result = await hallManager.UpdateHallAsync(model, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error == ConferenceHallErrors.NotFound
            ? NotFound(result.MapToApiResponse())
            : BadRequest(result.MapToApiResponse());
    }

    [HttpDelete("{hallId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConferenceHall(Guid hallId, CancellationToken cancellationToken)
    {
        var result = await hallManager.RemoveHallAsync(new HallReferenceModel(hallId), cancellationToken);
        if (result.IsSuccess) return NoContent();
        return result.Error == ConferenceHallErrors.NotFound
            ? NotFound(result.MapToApiResponse())
            : Conflict(result.MapToApiResponse());
    }

    [HttpGet("available")]
    [ProducesResponseType<ApiResponse<IEnumerable<ConferenceHallDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<IEnumerable<ConferenceHallDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableConferenceHalls(
        [FromQuery] GetAvailableConferenceHallsRequest request,
        CancellationToken cancellationToken)
    {
        var model = new FindAvailableHallsModel(
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Capacity);
        var result = await hallManager.GetAvailableHallsAsync(model, cancellationToken);
        var response = result.MapToApiResponse(mapper.Map<IEnumerable<ConferenceHallDto>>);

        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
