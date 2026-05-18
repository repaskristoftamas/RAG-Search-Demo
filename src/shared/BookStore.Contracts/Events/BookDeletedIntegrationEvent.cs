namespace BookStore.Contracts.Events;

/// <summary>
/// Integration event published when a book is removed from the catalog.
/// </summary>
/// <param name="BookId">The unique identifier of the deleted book.</param>
/// <param name="OccurredAt">UTC timestamp when the event was raised.</param>
public sealed record BookDeletedIntegrationEvent(
    Guid BookId,
    DateTimeOffset OccurredAt);
