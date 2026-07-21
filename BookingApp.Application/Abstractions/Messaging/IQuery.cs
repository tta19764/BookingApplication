using BookingApp.Domain.Abstractions;
using MediatR;

namespace BookingApp.Application.Abstractions.Messaging;

/// <summary>
/// Represents an application request that reads data without changing state.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}