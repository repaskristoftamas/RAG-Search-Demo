using BookStore.Contracts.Events;
using BookStore.KeywordSearch.Application.Abstractions;
using BookStore.KeywordSearch.Application.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BookStore.KeywordSearch.Application.Consumers;

/// <summary>
/// Consumes <see cref="BookUpdatedIntegrationEvent"/> and updates the searchable book record.
/// </summary>
public sealed class BookUpdatedConsumer(
    IKeywordSearchDbContext context) : IConsumer<BookUpdatedIntegrationEvent>
{
    /// <summary>
    /// Handles the book updated event by re-indexing the book.
    /// </summary>
    public async Task Consume(ConsumeContext<BookUpdatedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;

        var existing = await context.SearchableBooks
            .FirstOrDefaultAsync(b => b.Id == message.BookId, consumeContext.CancellationToken);

        if (existing is not null)
        {
            existing.Title = message.Title;
            existing.Author = message.Author;
            existing.Description = message.Description;
            existing.IndexedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            context.SearchableBooks.Add(new SearchableBook
            {
                Id = message.BookId,
                Title = message.Title,
                Author = message.Author,
                Description = message.Description,
                IndexedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync(consumeContext.CancellationToken);
    }
}
