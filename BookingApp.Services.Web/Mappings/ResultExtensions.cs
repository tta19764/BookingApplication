using BookingApp.Services.Web.Dtos;
using BookingApp.Bll.Common.Shared;

namespace BookingApp.Services.Web.Mappings;

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

    public static ApiResponse<TDto> MapToApiResponse<TModel, TDto>(
        this Result<TModel> result,
        Func<TModel, TDto> map)
    {
        return result.IsSuccess
            ? new ApiResponse<TDto> { Data = map(result.Value) }
            : new ApiResponse<TDto> { Error = result.Error };
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
