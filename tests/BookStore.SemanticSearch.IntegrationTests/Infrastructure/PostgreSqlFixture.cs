using Testcontainers.PostgreSql;

namespace BookStore.SemanticSearch.IntegrationTests.Infrastructure;

/// <summary>
/// Manages a shared PostgreSQL container with pgvector for all semantic-search integration tests.
/// </summary>
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("pgvector/pgvector:pg17")
        .Build();

    /// <summary>
    /// Connection string to the running PostgreSQL container.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

[CollectionDefinition(nameof(PostgreSqlCollection))]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>;
