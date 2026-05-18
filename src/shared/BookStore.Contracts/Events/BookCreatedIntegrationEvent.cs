namespace BookStore.Contracts.Events;

/// <summary>
/// Integration event published when a new book is added to the catalog.
/// </summary>
/// <param name="BookId">The unique identifier of the created book.</param>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Author name.</param>
/// <param name="Description">Short content description of the book.</param>
/// <param name="OccurredAt">UTC timestamp when the event was raised.</param>
public sealed record BookCreatedIntegrationEvent(
    Guid BookId,
    string Title,
    string Author,
    string Description,
    DateTimeOffset OccurredAt);
