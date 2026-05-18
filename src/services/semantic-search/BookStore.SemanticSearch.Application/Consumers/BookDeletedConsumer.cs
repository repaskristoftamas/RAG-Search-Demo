using BookStore.Contracts.Events;
using BookStore.SemanticSearch.Application.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BookStore.SemanticSearch.Application.Consumers;

/// <summary>
/// Consumes <see cref="BookDeletedIntegrationEvent"/> and removes the embedding record.
/// </summary>
public sealed class BookDeletedConsumer(
    ISemanticSearchDbContext context) : IConsumer<BookDeletedIntegrationEvent>
{
    /// <summary>
    /// Handles the book deleted event by removing the embedding from the index.
    /// </summary>
    public async Task Consume(ConsumeContext<BookDeletedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;

        var existing = await context.BookEmbeddings
            .FirstOrDefaultAsync(b => b.Id == message.BookId, consumeContext.CancellationToken);

        if (existing is not null)
        {
            context.BookEmbeddings.Remove(existing);
            await context.SaveChangesAsync(consumeContext.CancellationToken);
        }
    }
}
