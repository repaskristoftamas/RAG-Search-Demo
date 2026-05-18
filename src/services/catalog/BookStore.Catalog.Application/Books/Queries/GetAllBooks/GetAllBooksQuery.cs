using BookStore.Catalog.Application.Books.DTOs;
using BookStore.SharedKernel.Pagination;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Queries.GetAllBooks;

/// <summary>
/// Query to retrieve a paginated list of all books in the catalog.
/// </summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of results per page.</param>
public sealed record GetAllBooksQuery(int Page, int PageSize) : IQuery<Result<PagedResult<BookDto>>>;
