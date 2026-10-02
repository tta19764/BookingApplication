namespace BookingApp.Application.Abstractions.Messaging;

/// <summary>
/// Handles an application request without coupling business logic to a mediator library.
/// </summary>
public interface IApplicationRequestHandler<in TRequest, TResponse>
    where TRequest : IApplicationRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
