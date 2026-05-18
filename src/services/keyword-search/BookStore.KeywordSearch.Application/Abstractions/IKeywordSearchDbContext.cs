using BookStore.KeywordSearch.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.KeywordSearch.Application.Abstractions;

/// <summary>
/// Abstraction over the keyword search database context.
/// </summary>
public interface IKeywordSearchDbContext
{
    /// <summary>
    /// Queryable set of searchable books.
    /// </summary>
    DbSet<SearchableBook> SearchableBooks { get; }

    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
