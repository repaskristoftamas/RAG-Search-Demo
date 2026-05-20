using BookStore.Catalog.Application.Books.Commands.SeedBooks;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Handlers;

[Collection(nameof(PostgreSqlCollection))]
public sealed class SeedBooksCommandHandlerTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_NewBooks_InsertsAllAndReturnsCount()
    {
        await using var context = CreateContext();
        var handler = new SeedBooksCommandHandler(context);
        var command = new SeedBooksCommand(
        [
            new SeedBookItem("Book A", "Author A", "Description A"),
            new SeedBookItem("Book B", "Author B", "Description B")
        ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_NewBooks_PersistsAllToDatabase()
    {
        await using (var context = CreateContext())
        {
            var handler = new SeedBooksCommandHandler(context);
            var command = new SeedBooksCommand(
            [
                new SeedBookItem("Book A", "Author A", "Description A"),
                new SeedBookItem("Book B", "Author B", "Description B")
            ]);

            await handler.Handle(command, CancellationToken.None);
        }

        await using (var context = CreateContext())
        {
            var count = await context.Books.CountAsync();
            count.ShouldBe(2);
        }
    }

    [Fact]
    public async Task Handle_DuplicateTitle_SkipsDuplicate()
    {
        await InsertBookAsync("Existing Book");

        await using var context = CreateContext();
        var handler = new SeedBooksCommandHandler(context);
        var command = new SeedBooksCommand(
        [
            new SeedBookItem("Existing Book", "Author", "Description"),
            new SeedBookItem("New Book", "Author", "Description")
        ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveDuplicateTitle_SkipsDuplicate()
    {
        await InsertBookAsync("Clean Code");

        await using var context = CreateContext();
        var handler = new SeedBooksCommandHandler(context);
        var command = new SeedBooksCommand([new SeedBookItem("CLEAN CODE", "Author", "Desc")]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_InvalidBookData_SkipsInvalidAndInsertsValid()
    {
        await using var context = CreateContext();
        var handler = new SeedBooksCommandHandler(context);
        var command = new SeedBooksCommand(
        [
            new SeedBookItem("", "Author", "Description"),
            new SeedBookItem("Valid Book", "Author", "Description")
        ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_AllDuplicates_ReturnsZero()
    {
        await InsertBookAsync("Only Book");

        await using var context = CreateContext();
        var handler = new SeedBooksCommandHandler(context);
        var command = new SeedBooksCommand([new SeedBookItem("Only Book", "Author", "Desc")]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_EmptyBookList_ReturnsZero()
    {
        await using var context = CreateContext();
        var handler = new SeedBooksCommandHandler(context);
        var command = new SeedBooksCommand([]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ShouldBe(0);
    }
}
