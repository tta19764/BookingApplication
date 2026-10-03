using Asp.Versioning;
using AutoMapper;
using BookingApp.Bll.Common.Reports;
using BookingApp.Services.Web.Dtos;
using BookingApp.Services.Web.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Services.Web.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/reports")]
public sealed class ReportsController(IReportManager reportManager, IMapper mapper) : ControllerBase
{
    [HttpGet("bookings-summary")]
    [ProducesResponseType<ApiResponse<BookingSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookingSummary(CancellationToken cancellationToken)
    {
        var result = await reportManager.GetBookingSummaryAsync(cancellationToken);
        return Ok(result.MapToApiResponse(mapper.Map<BookingSummaryDto>));
    }
}
