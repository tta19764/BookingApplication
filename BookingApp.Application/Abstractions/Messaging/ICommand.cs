using BookingApp.Domain.Abstractions;

namespace BookingApp.Application.Abstractions.Messaging;

/// <summary>
/// Represents an application request that changes state and returns only success or failure.
/// </summary>
public interface ICommand : IApplicationRequest<Result>, IBaseCommand
{
}

/// <summary>
/// Represents an application request that changes state and returns a response payload.
/// </summary>
public interface ICommand<TResponse> : IApplicationRequest<Result<TResponse>>, IBaseCommand
{
}

/// <summary>
/// Marker interface used to identify requests that belong to the command pipeline.
/// </summary>
public interface IBaseCommand
{
}
