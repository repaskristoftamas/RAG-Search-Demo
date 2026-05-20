using BookStore.Catalog.Application.Books.Queries.GetAllBooks;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class GetAllBooksQueryHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_BooksExist_ReturnsPagedResult()
    {
        for (var i = 1; i <= 5; i++)
            await InsertBookAsync($"Book {i}", $"Author {i}", $"Description {i}");

        await using var context = CreateContext();
        var handler = new GetAllBooksQueryHandler(context);

        var result = await handler.Handle(
            new GetAllBooksQuery(Page: 1, PageSize: 10),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var paged = result.Value;
        paged.TotalCount.ShouldBe(5);
        paged.Items.Count.ShouldBe(5);
        paged.Page.ShouldBe(1);
        paged.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_NoBooksExist_ReturnsEmptyPage()
    {
        await using var context = CreateContext();
        var handler = new GetAllBooksQueryHandler(context);

        var result = await handler.Handle(
            new GetAllBooksQuery(Page: 1, PageSize: 10),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var paged = result.Value;
        paged.TotalCount.ShouldBe(0);
        paged.Items.ShouldBeEmpty();
        paged.TotalPages.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsCorrectSlice()
    {
        for (var i = 1; i <= 5; i++)
            await InsertBookAsync($"Book {i:D2}", $"Author {i}", $"Description {i}");

        await using var context = CreateContext();
        var handler = new GetAllBooksQueryHandler(context);

        var result = await handler.Handle(
            new GetAllBooksQuery(Page: 2, PageSize: 2),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var paged = result.Value;
        paged.TotalCount.ShouldBe(5);
        paged.Items.Count.ShouldBe(2);
        paged.HasNextPage.ShouldBeTrue();
        paged.HasPreviousPage.ShouldBeTrue();
    }
}
