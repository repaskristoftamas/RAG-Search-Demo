using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Application.Books.DTOs;
using BookStore.Catalog.Application.Books.Mappers;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Queries.GetBookById;

/// <summary>
/// Handles retrieval of a single book by its identifier.
/// </summary>
internal sealed class GetBookByIdQueryHandler(
    ICatalogDbContext context) : IQueryHandler<GetBookByIdQuery, Result<BookDto>>
{
    /// <summary>
    /// Retrieves the book or returns a not-found error.
    /// </summary>
    public async ValueTask<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await context.Books.FindAsync([query.Id], cancellationToken);
        if (book is null)
            return Result.Failure<BookDto>(new NotFoundError(BookErrorCodes.NotFound, "The book with the specified identifier was not found."));

        return Result.Success(book.ToDto());
    }
}
