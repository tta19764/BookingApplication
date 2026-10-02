using BookingApp.Bll.Common.Abstractions;

namespace BookingApp.Bll.Abstractions.Messaging;

/// <summary>
/// Represents an application request that changes state and returns only success or failure.
/// </summary>
public interface IManagerRequest : IManagerRequest<Result>
{
}

/// <summary>
/// Represents an application request that changes state and returns a response payload.
/// </summary>
public interface IManagerRequest<TResponse>
{
}

/// <summary>
/// Marker interface used to identify requests that belong to the command pipeline.
/// </summary>
