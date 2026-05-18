using BookStore.KeywordSearch.Application.Abstractions;
using BookStore.KeywordSearch.Application.Consumers;
using BookStore.KeywordSearch.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.KeywordSearch.Infrastructure;

/// <summary>
/// Registers infrastructure-layer services for the keyword search service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the EF Core context, MassTransit consumers, and health checks.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KeywordDb");

        services.AddDbContext<KeywordSearchDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IKeywordSearchDbContext>(sp =>
            sp.GetRequiredService<KeywordSearchDbContext>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<BookCreatedConsumer>();
            x.AddConsumer<BookUpdatedConsumer>();
            x.AddConsumer<BookDeletedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHealthChecks()
            .AddDbContextCheck<KeywordSearchDbContext>(tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Applies pending EF Core migrations and creates the tsvector column + GIN index.
    /// </summary>
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KeywordSearchDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        // Create the generated tsvector column and GIN index if they don't exist
        await dbContext.Database.ExecuteSqlRawAsync("""
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'SearchableBooks' AND column_name = 'SearchVector'
                ) THEN
                    ALTER TABLE "SearchableBooks"
                    ADD COLUMN "SearchVector" tsvector GENERATED ALWAYS AS (
                        setweight(to_tsvector('english', "Title"), 'A') ||
                        setweight(to_tsvector('english', "Author"), 'B') ||
                        setweight(to_tsvector('english', "Description"), 'C')
                    ) STORED;
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_indexes
                    WHERE indexname = 'idx_searchable_books_search'
                ) THEN
                    CREATE INDEX idx_searchable_books_search ON "SearchableBooks" USING GIN ("SearchVector");
                END IF;
            END $$;
            """, cancellationToken);
    }
}
