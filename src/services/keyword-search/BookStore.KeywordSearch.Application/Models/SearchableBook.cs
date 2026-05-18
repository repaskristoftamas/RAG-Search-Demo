namespace BookStore.KeywordSearch.Application.Models;

/// <summary>
/// Denormalized read model for full-text search. Not a DDD entity — this is a read-side projection.
/// </summary>
public sealed class SearchableBook
{
    /// <summary>
    /// Book identifier matching the catalog source.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Short content description of the book.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this book was indexed.
    /// </summary>
    public DateTimeOffset IndexedAt { get; set; }
}
