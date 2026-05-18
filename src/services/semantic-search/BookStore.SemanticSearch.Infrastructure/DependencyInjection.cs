using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Consumers;
using BookStore.SemanticSearch.Infrastructure.Data;
using BookStore.SemanticSearch.Infrastructure.Embedding;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace BookStore.SemanticSearch.Infrastructure;

/// <summary>
/// Registers infrastructure-layer services for the semantic search service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the EF Core context with pgvector, MassTransit consumers, Ollama services, and health checks.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SemanticDb");

        services.AddDbContext<SemanticSearchDbContext>(options =>
            options.UseNpgsql(connectionString, o => o.UseVector()));

        services.AddScoped<ISemanticSearchDbContext>(sp =>
            sp.GetRequiredService<SemanticSearchDbContext>());

        var ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var embeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        var generationModel = configuration["Ollama:GenerationModel"] ?? "llama3.2";

        var ollamaClient = new OllamaApiClient(new Uri(ollamaBaseUrl));

        services.AddSingleton<IEmbeddingService>(new OllamaEmbeddingService(ollamaClient, embeddingModel));
        services.AddSingleton<ITextGenerationService>(new OllamaTextGenerationService(ollamaClient, generationModel));

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
            .AddDbContextCheck<SemanticSearchDbContext>(tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Applies pending EF Core migrations.
    /// </summary>
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SemanticSearchDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
