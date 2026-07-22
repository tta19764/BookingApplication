using BookingApp.Domain.Abstractions;

namespace BookingApp.Api.Contracts;

/// <summary>
/// Standard response envelope returned by minimal API endpoints.
/// </summary>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Successful response payload.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error returned when the operation fails.
    /// </summary>
    public Error? Error { get; init; }
}
