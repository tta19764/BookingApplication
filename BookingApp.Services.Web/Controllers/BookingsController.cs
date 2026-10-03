using Asp.Versioning;
using AutoMapper;
using BookingApp.Bll.Common.Bookings;
using BookingApp.Bll.Common.Bookings.Models;
using BookingApp.Bll.Common.Shared.Models;
using BookingApp.Services.Web.Configuration;
using BookingApp.Services.Web.Dtos;
using BookingApp.Services.Web.Dtos.Requests;
using BookingApp.Services.Web.Mappings;
using BookingApp.Services.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Services.Web.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/bookings")]
public sealed class BookingsController(IBookingManager bookingManager, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyCollection<BookingDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<IReadOnlyCollection<BookingDto>>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBookings(
        [FromQuery] GetBookingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await bookingManager.GetBookingsAsync(
            new PaginationModel(request.Page, request.PageSize),
            cancellationToken);

        var response = result.MapToApiResponse(mapper.Map<IReadOnlyCollection<BookingDto>>);
        return result.IsSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<BookingConfirmationDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<BookingConfirmationDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<BookingConfirmationDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var model = new CreateBookingModel(
            request.HallId,
            SeedDataExtensions.SeededUserId,
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Amenities);
        var result = await bookingManager.AddBookingAsync(model, cancellationToken);
        var response = result.MapToApiResponse(mapper.Map<BookingConfirmationDto>);

        if (result.IsSuccess)
        {
            return Created(
                $"/api/v{BookingAppApiVersions.V1RouteValue}/bookings/{result.Value.BookingId}",
                response);
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? NotFound(response)
            : BadRequest(response);
    }
}
