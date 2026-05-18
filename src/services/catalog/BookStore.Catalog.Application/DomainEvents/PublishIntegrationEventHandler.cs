using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;
using BookStore.Contracts.Events;
using MassTransit;
using Mediator;

namespace BookStore.Catalog.Application.DomainEvents;

/// <summary>
/// Bridges in-process domain events to integration events published via MassTransit.
/// </summary>
internal sealed class PublishIntegrationEventHandler(
    IPublishEndpoint publishEndpoint,
    ICatalogDbContext context) : INotificationHandler<DomainEventNotification>
{
    /// <summary>
    /// Translates domain events into integration events and publishes them to the message broker.
    /// </summary>
    public async ValueTask Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        switch (notification.DomainEvent)
        {
            case BookCreatedEvent created:
            {
                var book = await context.Books.FindAsync([created.BookId], cancellationToken);
                if (book is null) return;

                await publishEndpoint.Publish(new BookCreatedIntegrationEvent(
                    book.Id.Value, book.Title, book.Author, book.Description, now), cancellationToken);
                break;
            }

            case BookUpdatedEvent updated:
            {
                var book = await context.Books.FindAsync([updated.BookId], cancellationToken);
                if (book is null) return;

                await publishEndpoint.Publish(new BookUpdatedIntegrationEvent(
                    book.Id.Value, book.Title, book.Author, book.Description, now), cancellationToken);
                break;
            }

            case BookDeletedEvent deleted:
            {
                await publishEndpoint.Publish(new BookDeletedIntegrationEvent(
                    deleted.BookId.Value, now), cancellationToken);
                break;
            }
        }
    }
}
