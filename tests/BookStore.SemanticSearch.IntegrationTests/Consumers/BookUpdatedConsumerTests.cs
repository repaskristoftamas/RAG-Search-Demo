using BookStore.Contracts.Events;
using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Consumers;
using BookStore.SemanticSearch.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.SemanticSearch.IntegrationTests.Consumers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookUpdatedConsumerTests(PostgreSqlFixture fixture)
    : SemanticSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Consume_ExistingBook_UpdatesFields()
    {
        var bookId = Guid.NewGuid();
        var embedding = CreateEmbedding((0, 1.0f));
        await InsertBookEmbeddingAsync(bookId, "Old Title", "Old Author", "Old Description", embedding);

        var newEmbedding = CreateEmbedding((1, 1.0f));
        var mockEmbedding = CreateMockEmbeddingService(newEmbedding);

        await using (var context = CreateContext())
        {
            var consumer = new BookUpdatedConsumer(context, mockEmbedding.Object);
            var message = new BookUpdatedIntegrationEvent(
                bookId, "New Title", "New Author", "New Description", DateTimeOffset.UtcNow);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var book = await context.BookEmbeddings.SingleAsync(b => b.Id == bookId);
            book.Title.ShouldBe("New Title");
            book.Author.ShouldBe("New Author");
            book.Description.ShouldBe("New Description");
        }
    }

    [Fact]
    public async Task Consume_MissingBook_UpsertsNewRecord()
    {
        var bookId = Guid.NewGuid();
        var embedding = CreateEmbedding((0, 1.0f));
        var mockEmbedding = CreateMockEmbeddingService(embedding);

        await using (var context = CreateContext())
        {
            var consumer = new BookUpdatedConsumer(context, mockEmbedding.Object);
            var message = new BookUpdatedIntegrationEvent(
                bookId, "Upserted Title", "Upserted Author", "Upserted Desc", DateTimeOffset.UtcNow);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var book = await context.BookEmbeddings.SingleOrDefaultAsync(b => b.Id == bookId);
            book.ShouldNotBeNull();
            book.Title.ShouldBe("Upserted Title");
        }
    }

    private static Mock<IEmbeddingService> CreateMockEmbeddingService(float[] embedding)
    {
        var mock = new Mock<IEmbeddingService>();
        mock.Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);
        return mock;
    }
}
