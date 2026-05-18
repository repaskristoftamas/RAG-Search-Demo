using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Catalog.Infrastructure;

/// <summary>
/// Registers infrastructure-layer services into the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the EF Core database context, MassTransit messaging, and health checks.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICatalogDbContext>(sp =>
            sp.GetRequiredService<CatalogDbContext>());

        services.AddMassTransit(x =>
        {
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
            .AddDbContextCheck<CatalogDbContext>(tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Applies any pending EF Core migrations to the database.
    /// </summary>
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
