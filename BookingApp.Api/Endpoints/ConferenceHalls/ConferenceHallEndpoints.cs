using BookingApp.Api.Contracts;
using BookingApp.Api.Extensions;
using BookingApp.Application.ConferenceHalls.AddHall;
using BookingApp.Application.ConferenceHalls.GetAvailableHalls;
using BookingApp.Application.ConferenceHalls.GetHall;
using BookingApp.Application.ConferenceHalls.RemoveHall;
using BookingApp.Application.ConferenceHalls.UpdateHall;
using MediatR;
namespace BookingApp.Api.Endpoints.ConferenceHalls;

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

        group.MapGet("{hallId:guid}", GetConferenceHall)
            .WithName(nameof(GetConferenceHall))
            .WithSummary("Get conference hall details")
            .Produces<ApiResponse<HallResponse>>()
            .Produces<ApiResponse<HallResponse>>(StatusCodes.Status404NotFound);

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
            .Produces<ApiResponse<IEnumerable<HallResponse>>>()
            .Produces<ApiResponse<IEnumerable<HallResponse>>>(StatusCodes.Status400BadRequest);

        return builder;
    }

    public static async Task<IResult> CreateConferenceHall(
        AddHallCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(nameof(GetConferenceHall), new { hallId = result.Value, version = BookingAppApiVersions.V1RouteValue }, result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    public static async Task<IResult> GetConferenceHall(
        Guid hallId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHallQuery(hallId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    public static async Task<IResult> UpdateConferenceHall(
        Guid hallId,
        UpdateConferenceHallRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateHallCommand(
            hallId,
            request.Name,
            request.Capacity,
            request.HourlyRate,
            request.Amenities);

        var result = await sender.Send(command, cancellationToken);

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
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveHallCommand(hallId), cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.MapToApiResponse());
    }

    public static async Task<IResult> GetAvailableConferenceHalls(
        [AsParameters] GetAvailableConferenceHallsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableHallsQuery(
            request.Start,
            request.End,
            request.Capacity);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
