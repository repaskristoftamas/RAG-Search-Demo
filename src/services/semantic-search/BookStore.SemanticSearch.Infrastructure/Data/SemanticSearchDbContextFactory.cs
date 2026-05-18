using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookStore.SemanticSearch.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
internal sealed class SemanticSearchDbContextFactory : IDesignTimeDbContextFactory<SemanticSearchDbContext>
{
    /// <summary>
    /// Creates a new context configured for PostgreSQL with pgvector.
    /// </summary>
    public SemanticSearchDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SemanticSearchDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5435;Database=semantic_db;Username=semantic;Password=passWORD123",
            o => o.UseVector());
        return new SemanticSearchDbContext(optionsBuilder.Options);
    }
}
