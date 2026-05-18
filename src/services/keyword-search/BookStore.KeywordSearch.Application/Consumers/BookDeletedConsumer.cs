using BookStore.Contracts.Events;
using BookStore.KeywordSearch.Application.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BookStore.KeywordSearch.Application.Consumers;

/// <summary>
/// Consumes <see cref="BookDeletedIntegrationEvent"/> and removes the searchable book record.
/// </summary>
public sealed class BookDeletedConsumer(
    IKeywordSearchDbContext context) : IConsumer<BookDeletedIntegrationEvent>
{
    /// <summary>
    /// Handles the book deleted event by removing the book from the search index.
    /// </summary>
    public async Task Consume(ConsumeContext<BookDeletedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;

        var existing = await context.SearchableBooks
            .FirstOrDefaultAsync(b => b.Id == message.BookId, consumeContext.CancellationToken);

        if (existing is not null)
        {
            context.SearchableBooks.Remove(existing);
            await context.SaveChangesAsync(consumeContext.CancellationToken);
        }
    }
}
