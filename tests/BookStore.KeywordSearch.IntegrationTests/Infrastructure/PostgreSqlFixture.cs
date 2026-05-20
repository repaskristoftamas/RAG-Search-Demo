using Testcontainers.PostgreSql;

namespace BookStore.KeywordSearch.IntegrationTests.Infrastructure;

/// <summary>
/// Manages a shared PostgreSQL container for all keyword-search integration tests.
/// </summary>
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
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
