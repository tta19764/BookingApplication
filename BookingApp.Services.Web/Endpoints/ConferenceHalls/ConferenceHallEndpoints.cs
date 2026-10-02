using BookingApp.Services.Web.Contracts;
using BookingApp.Services.Web.Dtos;
using BookingApp.Services.Web.Extensions;
using BookingApp.Services.Web.Mappings;
using AutoMapper;
using BookingApp.Bll.ConferenceHalls.AddHall;
using BookingApp.Bll.ConferenceHalls.GetAvailableHalls;
using BookingApp.Bll.ConferenceHalls.GetHall;
using BookingApp.Bll.ConferenceHalls.GetHalls;
using BookingApp.Bll.ConferenceHalls.RemoveHall;
using BookingApp.Bll.ConferenceHalls.UpdateHall;
using BookingApp.Bll.Abstractions.Messaging;
namespace BookingApp.Services.Web.Endpoints.ConferenceHalls;

/// <summary>
/// Minimal API endpoints for conference hall management.
/// </summary>
public static class ConferenceHallEndpoints
{
    /// <summary>
    /// Maps conference hall endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapConferenceHallEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("conference-halls")
            .WithTags("Conference halls")
            .HasApiVersion(BookingAppApiVersions.V1);

        group.MapPost(string.Empty, CreateConferenceHall)
            .WithName(nameof(CreateConferenceHall))
            .WithSummary("Create a conference hall")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest);

        group.MapGet(string.Empty, GetConferenceHalls)
            .WithName(nameof(GetConferenceHalls))
            .WithSummary("Get conference halls by page")
            .Produces<ApiResponse<IReadOnlyCollection<ConferenceHallDto>>>()
            .Produces<ApiResponse<IReadOnlyCollection<ConferenceHallDto>>>(StatusCodes.Status400BadRequest);

        group.MapGet("{hallId:guid}", GetConferenceHall)
            .WithName(nameof(GetConferenceHall))
            .WithSummary("Get conference hall details")
            .Produces<ApiResponse<ConferenceHallDto>>()
            .Produces<ApiResponse<ConferenceHallDto>>(StatusCodes.Status404NotFound);

        group.MapPut("{hallId:guid}", UpdateConferenceHall)
            .WithName(nameof(UpdateConferenceHall))
            .WithSummary("Update conference hall details")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("{hallId:guid}", DeleteConferenceHall)
            .WithName(nameof(DeleteConferenceHall))
            .WithSummary("Delete a conference hall")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapGet("available", GetAvailableConferenceHalls)
            .WithName(nameof(GetAvailableConferenceHalls))
            .WithSummary("Find available conference halls")
            .Produces<ApiResponse<IEnumerable<ConferenceHallDto>>>()
            .Produces<ApiResponse<IEnumerable<ConferenceHallDto>>>(StatusCodes.Status400BadRequest);

        return builder;
    }

    public static async Task<IResult> GetConferenceHalls(
        [AsParameters] GetConferenceHallsRequest request,
        IManagerDispatcher dispatcher,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new GetHallsRequest(request.Page, request.PageSize),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse(mapper.Map<IReadOnlyCollection<ConferenceHallDto>>))
            : Results.BadRequest(result.MapToApiResponse(mapper.Map<IReadOnlyCollection<ConferenceHallDto>>));
    }

    public static async Task<IResult> CreateConferenceHall(
        AddHallRequest command,
        IManagerDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(nameof(GetConferenceHall), new { hallId = result.Value, version = BookingAppApiVersions.V1RouteValue }, result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    public static async Task<IResult> GetConferenceHall(
        Guid hallId,
        IManagerDispatcher dispatcher,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetHallRequest(hallId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse(mapper.Map<ConferenceHallDto>))
            : Results.NotFound(result.MapToApiResponse(mapper.Map<ConferenceHallDto>));
    }

    public static async Task<IResult> UpdateConferenceHall(
        Guid hallId,
        UpdateConferenceHallRequest request,
        IManagerDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateHallRequest(
            hallId,
            request.Name,
            request.Capacity,
            request.HourlyRate,
            request.Amenities);

        var result = await dispatcher.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    public static async Task<IResult> DeleteConferenceHall(
        Guid hallId,
        IManagerDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RemoveHallRequest(hallId), cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.MapToApiResponse());
    }

    public static async Task<IResult> GetAvailableConferenceHalls(
        [AsParameters] GetAvailableConferenceHallsRequest request,
        IManagerDispatcher dispatcher,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableHallsRequest(
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Capacity);

        var result = await dispatcher.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse(mapper.Map<IEnumerable<ConferenceHallDto>>))
            : Results.BadRequest(result.MapToApiResponse(mapper.Map<IEnumerable<ConferenceHallDto>>));
    }
}
