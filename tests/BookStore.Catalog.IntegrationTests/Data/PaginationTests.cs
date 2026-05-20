using BookStore.Catalog.Application.Books.Queries.GetAllBooks;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Data;

[Collection(nameof(PostgreSqlCollection))]
public sealed class PaginationTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetAllBooks_ReturnsCorrectPageCounts()
    {
        await using (var context = CreateContext())
        {
            for (var i = 1; i <= 7; i++)
            {
                var book = Book.Create($"Book {i:D2}", $"Author {i}", $"Description {i}").Value;
                context.Books.Add(book);
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var handler = new GetAllBooksQueryHandler(context);
            var result = await handler.Handle(new GetAllBooksQuery(Page: 1, PageSize: 3), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            var paged = result.Value;
            paged.TotalCount.ShouldBe(7);
            paged.TotalPages.ShouldBe(3);
            paged.Items.Count.ShouldBe(3);
            paged.HasNextPage.ShouldBeTrue();
            paged.HasPreviousPage.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task GetAllBooks_OrdersByTitle()
    {
        await using (var context = CreateContext())
        {
            context.Books.Add(Book.Create("Zebra", "Author", "Desc").Value);
            context.Books.Add(Book.Create("Alpha", "Author", "Desc").Value);
            context.Books.Add(Book.Create("Middle", "Author", "Desc").Value);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var handler = new GetAllBooksQueryHandler(context);
            var result = await handler.Handle(new GetAllBooksQuery(Page: 1, PageSize: 10), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            var titles = result.Value.Items.Select(b => b.Title).ToList();
            titles.ShouldBe(["Alpha", "Middle", "Zebra"]);
        }
    }

    [Fact]
    public async Task GetAllBooks_LastPage_ReturnsPartialResults()
    {
        await using (var context = CreateContext())
        {
            for (var i = 1; i <= 5; i++)
            {
                var book = Book.Create($"Book {i:D2}", $"Author {i}", $"Description {i}").Value;
                context.Books.Add(book);
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var handler = new GetAllBooksQueryHandler(context);
            var result = await handler.Handle(new GetAllBooksQuery(Page: 2, PageSize: 3), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            var paged = result.Value;
            paged.Items.Count.ShouldBe(2);
            paged.HasNextPage.ShouldBeFalse();
            paged.HasPreviousPage.ShouldBeTrue();
        }
    }
}
