using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;

namespace BookStore.Catalog.Domain.UnitTests.Books;

/// <summary>
/// Tests <see cref="SharedKernel.Abstractions.EntityBase{TId}"/> domain event behavior through the <see cref="Book"/> aggregate.
/// </summary>
public sealed class EntityBaseDomainEventTests
{
    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var book = Book.Create("Title", "Author", "Description").Value;
        book.DomainEvents.ShouldNotBeEmpty();

        book.ClearDomainEvents();

        book.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void AddDomainEvent_MultipleOperations_AccumulatesEventsInOrder()
    {
        var book = Book.Create("Title", "Author", "Description").Value;

        book.Update("New Title", "New Author", "New Description");

        book.DomainEvents.Count.ShouldBe(2);
        book.DomainEvents[0].ShouldBeOfType<BookCreatedEvent>();
        book.DomainEvents[1].ShouldBeOfType<BookUpdatedEvent>();
    }

    [Fact]
    public void ClearDomainEvents_ThenAddNew_ContainsOnlyNewEvents()
    {
        var book = Book.Create("Title", "Author", "Description").Value;
        book.ClearDomainEvents();

        book.Update("New Title", "New Author", "New Description");

        book.DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<BookUpdatedEvent>();
    }
}
