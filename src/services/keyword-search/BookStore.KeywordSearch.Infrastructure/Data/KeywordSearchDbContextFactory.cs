using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookStore.KeywordSearch.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
internal sealed class KeywordSearchDbContextFactory : IDesignTimeDbContextFactory<KeywordSearchDbContext>
{
    /// <summary>
    /// Creates a new context configured for PostgreSQL.
    /// </summary>
    public KeywordSearchDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KeywordSearchDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=keyword_db;Username=keyword;Password=passWORD123");
        return new KeywordSearchDbContext(optionsBuilder.Options);
    }
}
