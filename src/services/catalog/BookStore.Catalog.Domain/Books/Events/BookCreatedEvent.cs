using BookStore.SharedKernel.Abstractions;

namespace BookStore.Catalog.Domain.Books.Events;

/// <summary>
/// Raised when a new book is added to the catalog.
/// </summary>
/// <param name="BookId">The identifier of the created book.</param>
public sealed record BookCreatedEvent(BookId BookId) : IDomainEvent;
