using BookingApp.Domain.Abstractions;
using MediatR;

namespace BookingApp.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}