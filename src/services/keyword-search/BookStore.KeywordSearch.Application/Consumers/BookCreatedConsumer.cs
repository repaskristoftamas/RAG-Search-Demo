using BookStore.Contracts.Events;
using BookStore.KeywordSearch.Application.Abstractions;
using BookStore.KeywordSearch.Application.Models;
using MassTransit;

namespace BookStore.KeywordSearch.Application.Consumers;

/// <summary>
/// Consumes <see cref="BookCreatedIntegrationEvent"/> and inserts a searchable book record.
/// </summary>
public sealed class BookCreatedConsumer(
    IKeywordSearchDbContext context) : IConsumer<BookCreatedIntegrationEvent>
{
    /// <summary>
    /// Handles the book created event by indexing the book for keyword search.
    /// </summary>
    public async Task Consume(ConsumeContext<BookCreatedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;

        var searchableBook = new SearchableBook
        {
            Id = message.BookId,
            Title = message.Title,
            Author = message.Author,
            Description = message.Description,
            IndexedAt = DateTimeOffset.UtcNow
        };

        context.SearchableBooks.Add(searchableBook);
        await context.SaveChangesAsync(consumeContext.CancellationToken);
    }
}
