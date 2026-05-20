using BookStore.Contracts.Events;
using BookStore.KeywordSearch.Application.Consumers;
using BookStore.KeywordSearch.IntegrationTests.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.KeywordSearch.IntegrationTests.Consumers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookCreatedConsumerTests(PostgreSqlFixture fixture)
    : KeywordSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Consume_ValidEvent_CreatesSearchableBook()
    {
        var bookId = Guid.NewGuid();
        var message = new BookCreatedIntegrationEvent(
            bookId, "Clean Architecture", "Robert C. Martin",
            "A guide to software structure", DateTimeOffset.UtcNow);

        await using (var context = CreateContext())
        {
            var consumer = new BookCreatedConsumer(context);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var book = await context.SearchableBooks.SingleAsync(b => b.Id == bookId);
            book.Title.ShouldBe("Clean Architecture");
            book.Author.ShouldBe("Robert C. Martin");
            book.Description.ShouldBe("A guide to software structure");
            book.IndexedAt.ShouldNotBe(default);
        }
    }

    [Fact]
    public async Task Consume_MultipleBooksCreated_AllPersisted()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        await using (var context = CreateContext())
        {
            var consumer = new BookCreatedConsumer(context);

            for (var i = 0; i < ids.Length; i++)
            {
                var message = new BookCreatedIntegrationEvent(
                    ids[i], $"Book {i}", $"Author {i}", $"Description {i}", DateTimeOffset.UtcNow);
                await consumer.Consume(CreateConsumeContext(message));
            }
        }

        await using (var context = CreateContext())
        {
            var count = await context.SearchableBooks.CountAsync();
            count.ShouldBe(3);
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
