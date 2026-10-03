using BookingApp.Api.Contracts;
using BookingApp.Domain.Abstractions;

namespace BookingApp.Api.Extensions;

/// <summary>
/// Maps application result objects to API response envelopes.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a typed result into the standard API response shape.
    /// </summary>
    public static ApiResponse<T> MapToApiResponse<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new ApiResponse<T> { Data = result.Value }
            : new ApiResponse<T> { Error = result.Error };
    }

    /// <summary>
    /// Converts an untyped result into an API response with no payload.
    /// </summary>
    public static ApiResponse<object> MapToApiResponse(this Result result)
    {
        return result.IsSuccess
            ? new ApiResponse<object>()
            : new ApiResponse<object> { Error = result.Error };
    }
}
