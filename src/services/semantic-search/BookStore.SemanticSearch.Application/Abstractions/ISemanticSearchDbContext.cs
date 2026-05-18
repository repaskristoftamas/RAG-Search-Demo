using BookStore.SemanticSearch.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.SemanticSearch.Application.Abstractions;

/// <summary>
/// Abstraction over the semantic search database context.
/// </summary>
public interface ISemanticSearchDbContext
{
    /// <summary>
    /// Queryable set of book embeddings.
    /// </summary>
    DbSet<BookEmbedding> BookEmbeddings { get; }

    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
