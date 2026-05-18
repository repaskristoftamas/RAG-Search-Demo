using BookStore.KeywordSearch.Application.Abstractions;
using BookStore.KeywordSearch.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.KeywordSearch.Infrastructure.Data;

/// <summary>
/// EF Core database context for the keyword search service.
/// </summary>
public class KeywordSearchDbContext(
    DbContextOptions<KeywordSearchDbContext> options) : DbContext(options), IKeywordSearchDbContext
{
    /// <summary>
    /// Queryable set of searchable books.
    /// </summary>
    public DbSet<SearchableBook> SearchableBooks => Set<SearchableBook>();

    /// <summary>
    /// Applies entity configurations from this assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KeywordSearchDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
