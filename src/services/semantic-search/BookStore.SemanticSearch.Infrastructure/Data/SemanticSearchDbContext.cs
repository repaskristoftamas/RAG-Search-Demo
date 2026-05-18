using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.SemanticSearch.Infrastructure.Data;

/// <summary>
/// EF Core database context for the semantic search service with pgvector support.
/// </summary>
public class SemanticSearchDbContext(
    DbContextOptions<SemanticSearchDbContext> options) : DbContext(options), ISemanticSearchDbContext
{
    /// <summary>
    /// Queryable set of book embeddings.
    /// </summary>
    public DbSet<BookEmbedding> BookEmbeddings => Set<BookEmbedding>();

    /// <summary>
    /// Applies entity configurations and enables pgvector.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SemanticSearchDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
