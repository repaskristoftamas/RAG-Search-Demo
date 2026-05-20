using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Mediator;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Data;

[Collection(nameof(PostgreSqlCollection))]
public sealed class DomainEventDispatchTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveChanges_BookCreated_DispatchesDomainEventNotification()
    {
        var mockPublisher = new Mock<IPublisher>();
        mockPublisher.Setup(p => p.Publish(It.IsAny<DomainEventNotification>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        await using var context = CreateContext(publisher: mockPublisher.Object);
        var book = Book.Create("Title", "Author", "Description").Value;
        context.Books.Add(book);

        await context.SaveChangesAsync();

        mockPublisher.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification>(n => n.DomainEvent is BookCreatedEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChanges_BookUpdated_DispatchesDomainEventNotification()
    {
        var mockPublisher = new Mock<IPublisher>();
        mockPublisher.Setup(p => p.Publish(It.IsAny<DomainEventNotification>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        await using var context = CreateContext(publisher: mockPublisher.Object);
        var book = Book.Create("Title", "Author", "Description").Value;
        context.Books.Add(book);
        await context.SaveChangesAsync();

        mockPublisher.Invocations.Clear();
        book.Update("Updated", "Author", "Description");
        await context.SaveChangesAsync();

        mockPublisher.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification>(n => n.DomainEvent is BookUpdatedEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChanges_ClearsDomainEventsAfterDispatch()
    {
        var mockPublisher = new Mock<IPublisher>();
        mockPublisher.Setup(p => p.Publish(It.IsAny<DomainEventNotification>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        await using var context = CreateContext(publisher: mockPublisher.Object);
        var book = Book.Create("Title", "Author", "Description").Value;
        context.Books.Add(book);
        await context.SaveChangesAsync();

        book.DomainEvents.ShouldBeEmpty();
    }
}
