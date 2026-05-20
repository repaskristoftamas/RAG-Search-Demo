using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;

namespace BookStore.Catalog.Domain.UnitTests.Books;

public sealed class BookDeleteTests
{
    [Fact]
    public void Delete_EmitsBookDeletedEvent()
    {
        var book = Book.Create("Title", "Author", "Description").Value;
        book.ClearDomainEvents();

        book.Delete();

        book.DomainEvents.ShouldHaveSingleItem();
        var domainEvent = book.DomainEvents[0].ShouldBeOfType<BookDeletedEvent>();
        domainEvent.BookId.ShouldBe(book.Id);
    }
}
