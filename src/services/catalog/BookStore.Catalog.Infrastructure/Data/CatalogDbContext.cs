using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Abstractions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Catalog.Infrastructure.Data;

/// <summary>
/// EF Core database context for the catalog service.
/// </summary>
public class CatalogDbContext(
    DbContextOptions<CatalogDbContext> options,
    TimeProvider timeProvider,
    IPublisher publisher) : DbContext(options), ICatalogDbContext
{
    /// <summary>
    /// Queryable set of books persisted in the data store.
    /// </summary>
    public DbSet<Book> Books => Set<Book>();

    /// <summary>
    /// Applies entity configurations from the infrastructure assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists all pending changes and dispatches domain events after successful save.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = utcNow;
        }

        var entries = ChangeTracker.Entries<IHasDomainEvents>().ToList();

        var domainEvents = entries
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entry in entries)
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);

        return result;
    }
}
