namespace BookingApp.Bll.Abstractions.Messaging;

/// <summary>
/// Provides the presentation layer with one entry point into application use cases.
/// </summary>
public interface IManagerDispatcher
{
    Task<TResponse> Send<TResponse>(
        IManagerRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
