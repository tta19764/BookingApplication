namespace BookingApp.Application.Abstractions.Messaging;

/// <summary>
/// Provides the presentation layer with one entry point into application use cases.
/// </summary>
public interface IApplicationDispatcher
{
    Task<TResponse> Send<TResponse>(
        IApplicationRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
