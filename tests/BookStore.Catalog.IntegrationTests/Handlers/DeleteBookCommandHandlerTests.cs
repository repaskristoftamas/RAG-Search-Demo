using BookStore.Catalog.Application.Books.Commands.DeleteBook;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using BookStore.SharedKernel.Results;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class DeleteBookCommandHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_ExistingBook_ReturnsSuccess()
    {
        var bookId = await InsertBookAsync();

        await using var context = CreateContext();
        var handler = new DeleteBookCommandHandler(context);

        var result = await handler.Handle(
            new DeleteBookCommand(bookId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ExistingBook_RemovesBookFromDatabase()
    {
        var bookId = await InsertBookAsync();

        await using (var context = CreateContext())
        {
            var handler = new DeleteBookCommandHandler(context);
            await handler.Handle(new DeleteBookCommand(bookId), CancellationToken.None);
        }

        await using (var context = CreateContext())
        {
            var book = await context.Books.FindAsync(bookId);
            book.ShouldBeNull();
        }
    }

    [Fact]
    public async Task Handle_BookNotFound_ReturnsNotFoundError()
    {
        await using var context = CreateContext();
        var handler = new DeleteBookCommandHandler(context);

        var result = await handler.Handle(
            new DeleteBookCommand(BookId.New()),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<NotFoundError>();
        error.Code.ShouldBe(BookErrorCodes.NotFound);
    }
}
