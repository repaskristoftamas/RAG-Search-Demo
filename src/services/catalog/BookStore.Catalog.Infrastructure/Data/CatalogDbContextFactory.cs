using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookStore.Catalog.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating <see cref="CatalogDbContext"/> during EF Core migrations.
/// </summary>
internal sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    /// <summary>
    /// Creates a new <see cref="CatalogDbContext"/> configured for PostgreSQL.
    /// </summary>
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=catalog_db;Username=catalog;Password=passWORD123");

        return new CatalogDbContext(optionsBuilder.Options, TimeProvider.System, null!);
    }
}
