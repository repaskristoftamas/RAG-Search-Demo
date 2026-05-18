using BookStore.Contracts.Events;
using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace BookStore.SemanticSearch.Application.Consumers;

/// <summary>
/// Consumes <see cref="BookUpdatedIntegrationEvent"/>, regenerates the embedding, and updates the record.
/// </summary>
public sealed class BookUpdatedConsumer(
    ISemanticSearchDbContext context,
    IEmbeddingService embeddingService) : IConsumer<BookUpdatedIntegrationEvent>
{
    /// <summary>
    /// Handles the book updated event by re-embedding and updating the record.
    /// </summary>
    public async Task Consume(ConsumeContext<BookUpdatedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;
        var text = $"{message.Title} by {message.Author}. {message.Description}";
        var embedding = await embeddingService.GenerateEmbeddingAsync(text, consumeContext.CancellationToken);

        var existing = await context.BookEmbeddings
            .FirstOrDefaultAsync(b => b.Id == message.BookId, consumeContext.CancellationToken);

        if (existing is not null)
        {
            existing.Title = message.Title;
            existing.Author = message.Author;
            existing.Description = message.Description;
            existing.Embedding = new Vector(embedding);
            existing.IndexedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            context.BookEmbeddings.Add(new BookEmbedding
            {
                Id = message.BookId,
                Title = message.Title,
                Author = message.Author,
                Description = message.Description,
                Embedding = new Vector(embedding),
                IndexedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync(consumeContext.CancellationToken);
    }
}
