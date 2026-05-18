namespace BookStore.Catalog.Application.Books.DTOs;

/// <summary>
/// Data transfer object representing a book returned from query operations.
/// </summary>
/// <param name="Id">Unique identifier of the book.</param>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Author name.</param>
/// <param name="Description">Short content description of the book.</param>
/// <param name="CreatedAt">Timestamp when the book was added to the catalog.</param>
/// <param name="UpdatedAt">Timestamp of the last update, or <c>null</c> if never updated.</param>
public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
