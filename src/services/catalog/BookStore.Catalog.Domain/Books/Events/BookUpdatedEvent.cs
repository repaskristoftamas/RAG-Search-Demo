using BookStore.SharedKernel.Abstractions;

namespace BookStore.Catalog.Domain.Books.Events;

/// <summary>
/// Raised when an existing book's details are modified.
/// </summary>
/// <param name="BookId">The identifier of the updated book.</param>
public sealed record BookUpdatedEvent(BookId BookId) : IDomainEvent;
