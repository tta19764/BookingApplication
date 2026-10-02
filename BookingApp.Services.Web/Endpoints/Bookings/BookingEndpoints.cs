using BookingApp.Services.Web.Contracts;
using BookingApp.Services.Web.Dtos;
using BookingApp.Services.Web.Extensions;
using BookingApp.Services.Web.Mappings;
using AutoMapper;
using BookingApp.Bll.Common.Bookings;

namespace BookingApp.Services.Web.Endpoints.Bookings;

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
            .Produces<ApiResponse<IReadOnlyCollection<BookingDto>>>()
            .Produces<ApiResponse<IReadOnlyCollection<BookingDto>>>(StatusCodes.Status400BadRequest);

        group.MapPost(string.Empty, CreateBooking)
            .WithName(nameof(CreateBooking))
            .WithSummary("Create a booking for the seeded user")
            .Produces<ApiResponse<BookingConfirmationDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<BookingConfirmationDto>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<BookingConfirmationDto>>(StatusCodes.Status404NotFound);

        return builder;
    }

    public static async Task<IResult> GetBookings(
        [AsParameters] GetBookingsRequest request,
        IBookingManager bookingManager,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await bookingManager.GetBookingsAsync(request.Page, request.PageSize, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse(mapper.Map<IReadOnlyCollection<BookingDto>>))
            : Results.BadRequest(result.MapToApiResponse(mapper.Map<IReadOnlyCollection<BookingDto>>));
    }

    public static async Task<IResult> CreateBooking(
        CreateBookingRequest request,
        IBookingManager bookingManager,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await bookingManager.AddBookingAsync(
            request.HallId,
            SeedDataExtensions.SeededUserId,
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Amenities,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Created($"/api/v{BookingAppApiVersions.V1RouteValue}/bookings/{result.Value.BookingId}", result.MapToApiResponse(mapper.Map<BookingConfirmationDto>));
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse(mapper.Map<BookingConfirmationDto>))
            : Results.BadRequest(result.MapToApiResponse(mapper.Map<BookingConfirmationDto>));
    }
}
