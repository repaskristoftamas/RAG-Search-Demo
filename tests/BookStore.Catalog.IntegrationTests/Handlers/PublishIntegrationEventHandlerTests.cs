using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Application.DomainEvents;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using BookStore.Contracts.Events;
using MassTransit;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class PublishIntegrationEventHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_BookCreatedEvent_PublishesBookCreatedIntegrationEvent()
    {
        var bookId = await InsertBookAsync("Clean Code", "Uncle Bob", "Craftsmanship.");
        var mockPublish = new Mock<IPublishEndpoint>();

        await using var context = CreateContext();
        var handler = new PublishIntegrationEventHandler(mockPublish.Object, context);

        await handler.Handle(
            new DomainEventNotification(new BookCreatedEvent(bookId)),
            CancellationToken.None);

        mockPublish.Verify(p => p.Publish(
            It.Is<BookCreatedIntegrationEvent>(e =>
                e.BookId == bookId.Value &&
                e.Title == "Clean Code" &&
                e.Author == "Uncle Bob" &&
                e.Description == "Craftsmanship."),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_BookCreatedEvent_BookNotFound_DoesNotPublish()
    {
        var mockPublish = new Mock<IPublishEndpoint>();

        await using var context = CreateContext();
        var handler = new PublishIntegrationEventHandler(mockPublish.Object, context);

        await handler.Handle(
            new DomainEventNotification(new BookCreatedEvent(BookId.New())),
            CancellationToken.None);

        mockPublish.Verify(
            p => p.Publish(It.IsAny<BookCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_BookUpdatedEvent_PublishesBookUpdatedIntegrationEvent()
    {
        var bookId = await InsertBookAsync("Clean Code", "Uncle Bob", "Craftsmanship.");
        var mockPublish = new Mock<IPublishEndpoint>();

        await using var context = CreateContext();
        var handler = new PublishIntegrationEventHandler(mockPublish.Object, context);

        await handler.Handle(
            new DomainEventNotification(new BookUpdatedEvent(bookId)),
            CancellationToken.None);

        mockPublish.Verify(p => p.Publish(
            It.Is<BookUpdatedIntegrationEvent>(e =>
                e.BookId == bookId.Value &&
                e.Title == "Clean Code"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_BookUpdatedEvent_BookNotFound_DoesNotPublish()
    {
        var mockPublish = new Mock<IPublishEndpoint>();

        await using var context = CreateContext();
        var handler = new PublishIntegrationEventHandler(mockPublish.Object, context);

        await handler.Handle(
            new DomainEventNotification(new BookUpdatedEvent(BookId.New())),
            CancellationToken.None);

        mockPublish.Verify(
            p => p.Publish(It.IsAny<BookUpdatedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_BookDeletedEvent_PublishesBookDeletedIntegrationEvent()
    {
        var bookId = BookId.New();
        var mockPublish = new Mock<IPublishEndpoint>();

        await using var context = CreateContext();
        var handler = new PublishIntegrationEventHandler(mockPublish.Object, context);

        await handler.Handle(
            new DomainEventNotification(new BookDeletedEvent(bookId)),
            CancellationToken.None);

        mockPublish.Verify(p => p.Publish(
            It.Is<BookDeletedIntegrationEvent>(e => e.BookId == bookId.Value),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
