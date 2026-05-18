using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Application.Books.DTOs;
using BookStore.Catalog.Application.Books.Mappers;
using BookStore.SharedKernel.Pagination;
using BookStore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Catalog.Application.Books.Queries.GetAllBooks;

/// <summary>
/// Handles retrieval of a paginated list of books.
/// </summary>
internal sealed class GetAllBooksQueryHandler(
    ICatalogDbContext context) : IQueryHandler<GetAllBooksQuery, Result<PagedResult<BookDto>>>
{
    /// <summary>
    /// Retrieves the requested page of books.
    /// </summary>
    public async ValueTask<Result<PagedResult<BookDto>>> Handle(GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        var totalCount = await context.Books.CountAsync(cancellationToken);

        var books = await context.Books
            .OrderBy(b => b.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = books.Select(b => b.ToDto()).ToList();

        return Result.Success(new PagedResult<BookDto>(dtos, totalCount, query.Page, query.PageSize));
    }
}
