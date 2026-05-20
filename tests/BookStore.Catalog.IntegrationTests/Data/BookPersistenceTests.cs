using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Data;

[Collection(nameof(PostgreSqlCollection))]
public sealed class BookPersistenceTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateBook_PersistsAndRetrievesAllFields()
    {
        var book = Book.Create("Clean Architecture", "Robert C. Martin", "A guide to architecture.").Value;

        await using (var context = CreateContext())
        {
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var persisted = await context.Books.FindAsync(book.Id);

            persisted.ShouldNotBeNull();
            persisted.Title.ShouldBe("Clean Architecture");
            persisted.Author.ShouldBe("Robert C. Martin");
            persisted.Description.ShouldBe("A guide to architecture.");
        }
    }

    [Fact]
    public async Task UpdateBook_PersistsChanges()
    {
        var book = Book.Create("Original", "Author", "Description").Value;

        await using (var context = CreateContext())
        {
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var tracked = await context.Books.FindAsync(book.Id);
            tracked!.Update("Updated", "New Author", "New Description");
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var persisted = await context.Books.FindAsync(book.Id);

            persisted.ShouldNotBeNull();
            persisted.Title.ShouldBe("Updated");
            persisted.Author.ShouldBe("New Author");
            persisted.Description.ShouldBe("New Description");
        }
    }

    [Fact]
    public async Task DeleteBook_RemovesFromDatabase()
    {
        var book = Book.Create("To Delete", "Author", "Description").Value;

        await using (var context = CreateContext())
        {
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var tracked = await context.Books.FindAsync(book.Id);
            tracked!.Delete();
            context.Books.Remove(tracked);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var persisted = await context.Books.FindAsync(book.Id);
            persisted.ShouldBeNull();
        }
    }
}
