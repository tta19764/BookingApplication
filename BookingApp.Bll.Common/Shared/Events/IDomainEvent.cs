namespace BookingApp.Bll.Common.Shared.Events;

/// <summary>
/// Marks a business fact that has already occurred in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>UTC timestamp at which the business event occurred.</summary>
    DateTime OccurredOnUtc { get; }
}
