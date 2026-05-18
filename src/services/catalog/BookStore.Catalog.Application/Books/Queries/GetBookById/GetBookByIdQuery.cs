using BookStore.Catalog.Application.Books.DTOs;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Queries.GetBookById;

/// <summary>
/// Query to retrieve a single book by its identifier.
/// </summary>
/// <param name="Id">The identifier of the book to retrieve.</param>
public sealed record GetBookByIdQuery(BookId Id) : IQuery<Result<BookDto>>;
