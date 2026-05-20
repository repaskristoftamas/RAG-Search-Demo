using BookStore.Contracts.Events;
using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Consumers;
using BookStore.SemanticSearch.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.SemanticSearch.IntegrationTests.Consumers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookCreatedConsumerTests(PostgreSqlFixture fixture)
    : SemanticSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Consume_ValidEvent_CreatesBookEmbedding()
    {
        var bookId = Guid.NewGuid();
        var embedding = CreateEmbedding((0, 1.0f));
        var mockEmbedding = CreateMockEmbeddingService(embedding);
        var message = new BookCreatedIntegrationEvent(
            bookId, "Clean Architecture", "Robert C. Martin",
            "A guide to software structure", DateTimeOffset.UtcNow);

        await using (var context = CreateContext())
        {
            var consumer = new BookCreatedConsumer(context, mockEmbedding.Object);
            await consumer.Consume(CreateConsumeContext(message));
        }

        await using (var context = CreateContext())
        {
            var book = await context.BookEmbeddings.SingleAsync(b => b.Id == bookId);
            book.Title.ShouldBe("Clean Architecture");
            book.Author.ShouldBe("Robert C. Martin");
            book.Description.ShouldBe("A guide to software structure");
            book.Embedding.ShouldNotBeNull();
            book.IndexedAt.ShouldNotBe(default);
        }
    }

    [Fact]
    public async Task Consume_MultipleBooksCreated_AllPersisted()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var embedding = CreateEmbedding((0, 1.0f));
        var mockEmbedding = CreateMockEmbeddingService(embedding);

        await using (var context = CreateContext())
        {
            var consumer = new BookCreatedConsumer(context, mockEmbedding.Object);

            for (var i = 0; i < ids.Length; i++)
            {
                var message = new BookCreatedIntegrationEvent(
                    ids[i], $"Book {i}", $"Author {i}", $"Description {i}", DateTimeOffset.UtcNow);
                await consumer.Consume(CreateConsumeContext(message));
            }
        }

        await using (var context = CreateContext())
        {
            var count = await context.BookEmbeddings.CountAsync();
            count.ShouldBe(3);
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
