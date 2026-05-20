using BookStore.Catalog.Application.Books.Queries.GetBookById;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using BookStore.SharedKernel.Results;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class GetBookByIdQueryHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_ExistingBook_ReturnsBookDto()
    {
        var bookId = await InsertBookAsync("Clean Code", "Uncle Bob", "Software craftsmanship.");

        await using var context = CreateContext();
        var handler = new GetBookByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetBookByIdQuery(bookId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        var dto = result.Value;
        dto.Id.ShouldBe(bookId.Value);
        dto.Title.ShouldBe("Clean Code");
        dto.Author.ShouldBe("Uncle Bob");
        dto.Description.ShouldBe("Software craftsmanship.");
    }

    [Fact]
    public async Task Handle_BookNotFound_ReturnsNotFoundError()
    {
        await using var context = CreateContext();
        var handler = new GetBookByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetBookByIdQuery(BookId.New()),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<NotFoundError>();
        error.Code.ShouldBe(BookErrorCodes.NotFound);
    }
}
