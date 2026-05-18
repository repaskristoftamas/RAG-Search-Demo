using BookStore.Contracts.Events;
using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Models;
using MassTransit;
using Pgvector;

namespace BookStore.SemanticSearch.Application.Consumers;

/// <summary>
/// Consumes <see cref="BookCreatedIntegrationEvent"/>, generates an embedding, and stores it.
/// </summary>
public sealed class BookCreatedConsumer(
    ISemanticSearchDbContext context,
    IEmbeddingService embeddingService) : IConsumer<BookCreatedIntegrationEvent>
{
    /// <summary>
    /// Handles the book created event by generating an embedding and indexing the book.
    /// </summary>
    public async Task Consume(ConsumeContext<BookCreatedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;
        var text = $"{message.Title} by {message.Author}. {message.Description}";
        var embedding = await embeddingService.GenerateEmbeddingAsync(text, consumeContext.CancellationToken);

        var bookEmbedding = new BookEmbedding
        {
            Id = message.BookId,
            Title = message.Title,
            Author = message.Author,
            Description = message.Description,
            Embedding = new Vector(embedding),
            IndexedAt = DateTimeOffset.UtcNow
        };

        context.BookEmbeddings.Add(bookEmbedding);
        await context.SaveChangesAsync(consumeContext.CancellationToken);
    }
}
