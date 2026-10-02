namespace BookingApp.Bll.Abstractions.Messaging;

/// <summary>
/// Handles an application request without coupling business logic to a mediator library.
/// </summary>
public interface IRequestManager<in TRequest, TResponse>
    where TRequest : IManagerRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
