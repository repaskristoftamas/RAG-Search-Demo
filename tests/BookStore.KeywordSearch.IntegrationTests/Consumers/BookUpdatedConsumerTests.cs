using BookStore.Contracts.Events;
using BookStore.KeywordSearch.Application.Consumers;
using BookStore.KeywordSearch.Application.Models;
using BookStore.KeywordSearch.IntegrationTests.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.KeywordSearch.IntegrationTests.Consumers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookUpdatedConsumerTests(PostgreSqlFixture fixture)
    : KeywordSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Consume_ExistingBook_UpdatesFields()
    {
        var bookId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            context.SearchableBooks.Add(new SearchableBook
            {
                Id = bookId,
                Title = "Old Title",
                Author = "Old Author",
                Description = "Old Description",
                IndexedAt = DateTimeOffset.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var consumer = new BookUpdatedConsumer(context);
            var message = new BookUpdatedIntegrationEvent(
                bookId, "New Title", "New Author", "New Description", DateTimeOffset.UtcNow);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var book = await context.SearchableBooks.SingleAsync(b => b.Id == bookId);
            book.Title.ShouldBe("New Title");
            book.Author.ShouldBe("New Author");
            book.Description.ShouldBe("New Description");
        }
    }

    [Fact]
    public async Task Consume_MissingBook_UpsertsNewRecord()
    {
        var bookId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var consumer = new BookUpdatedConsumer(context);
            var message = new BookUpdatedIntegrationEvent(
                bookId, "Upserted Title", "Upserted Author", "Upserted Desc", DateTimeOffset.UtcNow);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var book = await context.SearchableBooks.SingleOrDefaultAsync(b => b.Id == bookId);
            book.ShouldNotBeNull();
            book.Title.ShouldBe("Upserted Title");
        }
    }

    /// <summary>
    /// Creates a mock <see cref="ConsumeContext{T}"/> exposing the given message.
    /// </summary>
    private static ConsumeContext<T> CreateConsumeContext<T>(T message) where T : class
    {
        var mock = new Mock<ConsumeContext<T>>();
        mock.Setup(c => c.Message).Returns(message);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }
}
