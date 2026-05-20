using BookStore.Catalog.Application.Books.Commands.SeedBooks;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Data;

[Collection(nameof(PostgreSqlCollection))]
public sealed class SeedDeduplicationTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task SeedBooks_DuplicateTitles_DoesNotCreateDuplicates()
    {
        await using (var context = CreateContext())
        {
            var book = Book.Create("Existing Book", "Author", "Description").Value;
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var handler = new SeedBooksCommandHandler(context);
            var command = new SeedBooksCommand(
            [
                new SeedBookItem("Existing Book", "Same Author", "Different description"),
                new SeedBookItem("New Book", "New Author", "New description")
            ]);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Value.ShouldBe(1);
        }

        await using (var context = CreateContext())
        {
            var count = await context.Books.CountAsync();
            count.ShouldBe(2);
        }
    }

    [Fact]
    public async Task SeedBooks_SameTitlesTwice_NoDuplicatesOnSecondSeed()
    {
        var seedItems = new List<SeedBookItem>
        {
            new("Book A", "Author A", "Description A"),
            new("Book B", "Author B", "Description B")
        };

        await using (var context = CreateContext())
        {
            var handler = new SeedBooksCommandHandler(context);
            var result = await handler.Handle(new SeedBooksCommand(seedItems), CancellationToken.None);
            result.Value.ShouldBe(2);
        }

        await using (var context = CreateContext())
        {
            var handler = new SeedBooksCommandHandler(context);
            var result = await handler.Handle(new SeedBooksCommand(seedItems), CancellationToken.None);
            result.Value.ShouldBe(0);
        }

        await using (var context = CreateContext())
        {
            var count = await context.Books.CountAsync();
            count.ShouldBe(2);
        }
    }
}
