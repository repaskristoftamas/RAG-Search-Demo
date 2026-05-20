using BookStore.Contracts.Events;
using BookStore.KeywordSearch.Application.Consumers;
using BookStore.KeywordSearch.Application.Models;
using BookStore.KeywordSearch.IntegrationTests.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.KeywordSearch.IntegrationTests.Consumers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookDeletedConsumerTests(PostgreSqlFixture fixture)
    : KeywordSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Consume_ExistingBook_RemovesFromDatabase()
    {
        var bookId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            context.SearchableBooks.Add(new SearchableBook
            {
                Id = bookId,
                Title = "To Delete",
                Author = "Author",
                Description = "Description",
                IndexedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var consumer = new BookDeletedConsumer(context);
            var message = new BookDeletedIntegrationEvent(bookId, DateTimeOffset.UtcNow);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var exists = await context.SearchableBooks.AnyAsync(b => b.Id == bookId);
            exists.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Consume_MissingBook_DoesNotThrow()
    {
        var bookId = Guid.NewGuid();

        await using var context = CreateContext();
        var consumer = new BookDeletedConsumer(context);
        var message = new BookDeletedIntegrationEvent(bookId, DateTimeOffset.UtcNow);

        await Should.NotThrowAsync(() => consumer.Consume(CreateConsumeContext(message)));
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
