using BookStore.Catalog.Application.Books.Commands.UpdateBook;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using BookStore.SharedKernel.Results;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class UpdateBookCommandHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private const string UpdatedTitle = "Clean Code";
    private const string UpdatedAuthor = "Uncle Bob";
    private const string UpdatedDescription = "A handbook of agile software craftsmanship.";

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        var bookId = await InsertBookAsync();

        await using var context = CreateContext();
        var handler = new UpdateBookCommandHandler(context, new UpdateBookCommandValidator());

        var result = await handler.Handle(
            new UpdateBookCommand(bookId, UpdatedTitle, UpdatedAuthor, UpdatedDescription),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsUpdatedProperties()
    {
        var bookId = await InsertBookAsync();

        await using (var context = CreateContext())
        {
            var handler = new UpdateBookCommandHandler(context, new UpdateBookCommandValidator());

            await handler.Handle(
                new UpdateBookCommand(bookId, UpdatedTitle, UpdatedAuthor, UpdatedDescription),
                CancellationToken.None);
        }

        await using (var context = CreateContext())
        {
            var book = await context.Books.FindAsync(bookId);

            book.ShouldNotBeNull();
            book.Title.ShouldBe(UpdatedTitle);
            book.Author.ShouldBe(UpdatedAuthor);
            book.Description.ShouldBe(UpdatedDescription);
        }
    }

    [Fact]
    public async Task Handle_BookNotFound_ReturnsNotFoundError()
    {
        await using var context = CreateContext();
        var handler = new UpdateBookCommandHandler(context, new UpdateBookCommandValidator());

        var result = await handler.Handle(
            new UpdateBookCommand(BookId.New(), UpdatedTitle, UpdatedAuthor, UpdatedDescription),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<NotFoundError>();
        error.Code.ShouldBe(BookErrorCodes.NotFound);
    }

    [Fact]
    public async Task Handle_InvalidTitle_ReturnsValidationError()
    {
        await using var context = CreateContext();
        var handler = new UpdateBookCommandHandler(context, new UpdateBookCommandValidator());

        var result = await handler.Handle(
            new UpdateBookCommand(BookId.New(), "", UpdatedAuthor, UpdatedDescription),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>();
    }
}
