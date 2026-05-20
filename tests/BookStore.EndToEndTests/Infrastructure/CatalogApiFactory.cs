using BookStore.Catalog.Api.Endpoints;
using BookStore.Catalog.Infrastructure.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BookStore.EndToEndTests.Infrastructure;

/// <summary>
/// Creates a test server for the Catalog API backed by a real PostgreSQL container.
/// </summary>
public sealed class CatalogApiFactory : WebApplicationFactory<UpdateBookRequest>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CatalogDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(nameof(CatalogApiCollection))]
public sealed class CatalogApiCollection : ICollectionFixture<CatalogApiFactory>;
