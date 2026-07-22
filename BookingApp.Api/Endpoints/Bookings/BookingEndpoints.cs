using BookingApp.Api.Contracts;
using BookingApp.Api.Extensions;
using BookingApp.Application.Bookings.AddBooking;
using BookingApp.Application.Bookings.GetBookings;
using MediatR;

namespace BookingApp.Api.Endpoints.Bookings;

/// <summary>
/// Minimal API endpoints for booking conference halls.
/// </summary>
public static class BookingEndpoints
{
    /// <summary>
    /// Maps booking endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("bookings")
            .WithTags("Bookings")
            .HasApiVersion(BookingAppApiVersions.V1);

        group.MapGet(string.Empty, GetBookings)
            .WithName(nameof(GetBookings))
            .WithSummary("Get bookings by page")
            .Produces<ApiResponse<IReadOnlyCollection<BookingResponse>>>()
            .Produces<ApiResponse<IReadOnlyCollection<BookingResponse>>>(StatusCodes.Status400BadRequest);

        group.MapPost(string.Empty, CreateBooking)
            .WithName(nameof(CreateBooking))
            .WithSummary("Create a booking for the seeded user")
            .Produces<ApiResponse<BookingConfirmationResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<BookingConfirmationResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<BookingConfirmationResponse>>(StatusCodes.Status404NotFound);

        return builder;
    }

    public static async Task<IResult> GetBookings(
        [AsParameters] GetBookingsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetBookingsQuery(request.Page, request.PageSize),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    public static async Task<IResult> CreateBooking(
        CreateBookingRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddBookingCommand(
            request.HallId,
            SeedDataExtensions.SeededUserId,
            request.Start,
            request.End,
            request.Amenities);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Created($"/api/v{BookingAppApiVersions.V1RouteValue}/bookings/{result.Value.BookingId}", result.MapToApiResponse());
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
