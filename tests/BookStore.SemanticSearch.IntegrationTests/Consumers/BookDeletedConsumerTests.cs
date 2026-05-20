using BookStore.Contracts.Events;
using BookStore.SemanticSearch.Application.Consumers;
using BookStore.SemanticSearch.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.SemanticSearch.IntegrationTests.Consumers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookDeletedConsumerTests(PostgreSqlFixture fixture)
    : SemanticSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Consume_ExistingBook_RemovesFromDatabase()
    {
        var bookId = Guid.NewGuid();
        var embedding = CreateEmbedding((0, 1.0f));
        await InsertBookEmbeddingAsync(bookId, "To Delete", "Author", "Description", embedding);

        await using (var context = CreateContext())
        {
            var consumer = new BookDeletedConsumer(context);
            var message = new BookDeletedIntegrationEvent(bookId, DateTimeOffset.UtcNow);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var exists = await context.BookEmbeddings.AnyAsync(b => b.Id == bookId);
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
}
