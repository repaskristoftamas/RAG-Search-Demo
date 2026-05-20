using BookStore.Catalog.Application.Books.Commands.CreateBook;
using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using BookStore.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class CreateBookCommandHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private const string ValidTitle = "Clean Architecture";
    private const string ValidAuthor = "Robert C. Martin";
    private const string ValidDescription = "A comprehensive guide to software architecture.";

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithNonEmptyId()
    {
        await using var context = CreateContext();
        var handler = new CreateBookCommandHandler(context, new CreateBookCommandValidator());

        var result = await handler.Handle(
            new CreateBookCommand(ValidTitle, ValidAuthor, ValidDescription),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsBookToDatabase()
    {
        Guid bookId;
        await using (var context = CreateContext())
        {
            var handler = new CreateBookCommandHandler(context, new CreateBookCommandValidator());

            var result = await handler.Handle(
                new CreateBookCommand(ValidTitle, ValidAuthor, ValidDescription),
                CancellationToken.None);

            bookId = result.Value;
        }

        await using (var context = CreateContext())
        {
            var book = await context.Books.FindAsync(new BookId(bookId));

            book.ShouldNotBeNull();
            book.Title.ShouldBe(ValidTitle);
            book.Author.ShouldBe(ValidAuthor);
            book.Description.ShouldBe(ValidDescription);
        }
    }

    [Fact]
    public async Task Handle_InvalidTitle_ReturnsValidationError()
    {
        await using var context = CreateContext();
        var handler = new CreateBookCommandHandler(context, new CreateBookCommandValidator());

        var result = await handler.Handle(
            new CreateBookCommand("", ValidAuthor, ValidDescription),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldContain(f => f.Code == BookErrorCodes.TitleRequired);
    }

    [Fact]
    public async Task Handle_InvalidTitle_DoesNotPersistBook()
    {
        await using var context = CreateContext();
        var handler = new CreateBookCommandHandler(context, new CreateBookCommandValidator());

        await handler.Handle(
            new CreateBookCommand("", ValidAuthor, ValidDescription),
            CancellationToken.None);

        var count = await context.Books.CountAsync();
        count.ShouldBe(0);
    }
}
