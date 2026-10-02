using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.Abstractions.Messaging;

/// <summary>
/// Handles a query and returns a read model on success.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IApplicationRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
