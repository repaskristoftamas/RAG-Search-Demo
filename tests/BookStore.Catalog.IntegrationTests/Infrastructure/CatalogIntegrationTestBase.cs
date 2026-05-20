using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookStore.Catalog.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that creates an isolated database per test method.
/// </summary>
[Collection(nameof(PostgreSqlCollection))]
public abstract class CatalogIntegrationTestBase(PostgreSqlFixture fixture) : IAsyncLifetime
{
    private readonly string _databaseName = $"test_{Guid.NewGuid():N}";

    /// <summary>
    /// Creates a new <see cref="CatalogDbContext"/> pointing at the isolated test database.
    /// </summary>
    protected CatalogDbContext CreateContext(
        TimeProvider? timeProvider = null,
        IPublisher? publisher = null)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = _databaseName
        }.ToString();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CatalogDbContext(
            options,
            timeProvider ?? TimeProvider.System,
            publisher ?? Mock.Of<IPublisher>());
    }

    /// <summary>
    /// Inserts a book into the test database and returns its identifier.
    /// </summary>
    protected async Task<BookId> InsertBookAsync(
        string title = "Clean Architecture",
        string author = "Robert C. Martin",
        string description = "A comprehensive guide to software architecture.")
    {
        var book = Book.Create(title, author, description).Value;
        await using var context = CreateContext();
        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book.Id;
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
