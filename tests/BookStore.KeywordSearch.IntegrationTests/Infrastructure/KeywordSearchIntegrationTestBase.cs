using BookStore.KeywordSearch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookStore.KeywordSearch.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for keyword-search integration tests that creates an isolated database per test.
/// </summary>
[Collection(nameof(PostgreSqlCollection))]
public abstract class KeywordSearchIntegrationTestBase(PostgreSqlFixture fixture) : IAsyncLifetime
{
    private readonly string _databaseName = $"test_{Guid.NewGuid():N}";

    /// <summary>
    /// Creates a new <see cref="KeywordSearchDbContext"/> pointing at the isolated test database.
    /// </summary>
    protected KeywordSearchDbContext CreateContext()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = _databaseName
        }.ToString();

        var options = new DbContextOptionsBuilder<KeywordSearchDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new KeywordSearchDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("""
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
            """);
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
