namespace BookStore.Contracts.Events;

/// <summary>
/// Integration event published when book details are modified.
/// </summary>
/// <param name="BookId">The unique identifier of the updated book.</param>
/// <param name="Title">Updated title of the book.</param>
/// <param name="Author">Updated author name.</param>
/// <param name="Description">Updated short content description of the book.</param>
/// <param name="OccurredAt">UTC timestamp when the event was raised.</param>
public sealed record BookUpdatedIntegrationEvent(
    Guid BookId,
    string Title,
    string Author,
    string Description,
    DateTimeOffset OccurredAt);
