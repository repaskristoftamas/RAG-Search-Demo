using BookStore.SharedKernel.Abstractions;

namespace BookStore.Catalog.Domain.Books.Events;

/// <summary>
/// Raised when a book is removed from the catalog.
/// </summary>
/// <param name="BookId">The identifier of the deleted book.</param>
public sealed record BookDeletedEvent(BookId BookId) : IDomainEvent;
