using Pgvector;

namespace BookStore.SemanticSearch.Application.Models;

/// <summary>
/// Read model storing a book's text alongside its vector embedding for semantic search.
/// </summary>
public sealed class BookEmbedding
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
    /// Vector embedding of the combined title, author, and description text.
    /// </summary>
    public Vector Embedding { get; set; } = default!;

    /// <summary>
    /// Timestamp when this book was embedded and indexed.
    /// </summary>
    public DateTimeOffset IndexedAt { get; set; }
}
