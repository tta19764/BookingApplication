namespace BookingApp.Bll.Common.Shared.Events;

/// <summary>
/// Marks a business fact that has already occurred in the domain.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
