using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.DeleteBook;

/// <summary>
/// Handles deletion of a book from the catalog.
/// </summary>
internal sealed class DeleteBookCommandHandler(
    ICatalogDbContext context) : ICommandHandler<DeleteBookCommand, Result>
{
    /// <summary>
    /// Deletes the book with the specified identifier.
    /// </summary>
    public async ValueTask<Result> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var book = await context.Books.FindAsync([command.Id], cancellationToken);
        if (book is null)
            return Result.Failure(new NotFoundError(BookErrorCodes.NotFound, "The book with the specified identifier was not found."));

        book.Delete();
        context.Books.Remove(book);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
