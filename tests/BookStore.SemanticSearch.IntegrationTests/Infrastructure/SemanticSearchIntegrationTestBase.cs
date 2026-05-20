using BookStore.SemanticSearch.Application.Models;
using BookStore.SemanticSearch.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;

namespace BookStore.SemanticSearch.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for semantic-search integration tests that creates an isolated database per test.
/// </summary>
[Collection(nameof(PostgreSqlCollection))]
public abstract class SemanticSearchIntegrationTestBase(PostgreSqlFixture fixture) : IAsyncLifetime
{
    private readonly string _databaseName = $"test_{Guid.NewGuid():N}";

    /// <summary>
    /// Creates a new <see cref="SemanticSearchDbContext"/> pointing at the isolated test database.
    /// </summary>
    protected SemanticSearchDbContext CreateContext()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(fixture.ConnectionString)
        {
            Database = _databaseName
        }.ToString();

        var options = new DbContextOptionsBuilder<SemanticSearchDbContext>()
            .UseNpgsql(connectionString, o => o.UseVector())
            .Options;

        return new SemanticSearchDbContext(options);
    }

    /// <summary>
    /// Inserts a book embedding into the test database.
    /// </summary>
    protected async Task InsertBookEmbeddingAsync(
        Guid id,
        string title,
        string author,
        string description,
        float[] embedding)
    {
        await using var context = CreateContext();
        context.BookEmbeddings.Add(new BookEmbedding
        {
            Id = id,
            Title = title,
            Author = author,
            Description = description,
            Embedding = new Vector(embedding),
            IndexedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a 768-dimensional embedding with non-zero values at the specified indices.
    /// </summary>
    protected static float[] CreateEmbedding(params (int Index, float Value)[] components)
    {
        var embedding = new float[768];
        foreach (var (index, value) in components)
            embedding[index] = value;
        return embedding;
    }

    /// <summary>
    /// Creates a mock <see cref="ConsumeContext{T}"/> exposing the given message.
    /// </summary>
    protected static ConsumeContext<T> CreateConsumeContext<T>(T message) where T : class
    {
        var mock = new Mock<ConsumeContext<T>>();
        mock.Setup(c => c.Message).Returns(message);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
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
