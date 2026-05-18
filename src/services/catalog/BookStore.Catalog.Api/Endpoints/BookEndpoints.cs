using System.Text.Json;
using BookStore.Catalog.Api.Extensions;
using BookStore.Catalog.Application.Books.Commands.CreateBook;
using BookStore.Catalog.Application.Books.Commands.DeleteBook;
using BookStore.Catalog.Application.Books.Commands.SeedBooks;
using BookStore.Catalog.Application.Books.Commands.UpdateBook;
using BookStore.Catalog.Application.Books.DTOs;
using BookStore.Catalog.Application.Books.Queries.GetAllBooks;
using BookStore.Catalog.Application.Books.Queries.GetBookById;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Pagination;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Catalog.Api.Endpoints;

/// <summary>
/// Defines the CRUD endpoints for managing books in the catalog.
/// </summary>
public static class BookEndpoints
{
    /// <summary>
    /// Registers all book-related routes.
    /// </summary>
    public static void MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/books").WithTags("Books");

        group.MapGet("/", GetAllBooks).WithName("GetAllBooks");
        group.MapGet("/{id:guid}", GetBookById).WithName("GetBookById");
        group.MapPost("/", CreateBook).WithName("CreateBook");
        group.MapPut("/{id:guid}", UpdateBook).WithName("UpdateBook");
        group.MapDelete("/{id:guid}", DeleteBook).WithName("DeleteBook");
        group.MapPost("/seed", SeedBooks).WithName("SeedBooks");
    }

    /// <summary>
    /// Retrieves a page of books from the catalog.
    /// </summary>
    private static async Task<Results<Ok<PagedResult<BookDto>>, ProblemHttpResult>> GetAllBooks(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new GetAllBooksQuery(page, pageSize), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Retrieves a single book by its identifier.
    /// </summary>
    private static async Task<Results<Ok<BookDto>, ProblemHttpResult>> GetBookById(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookByIdQuery(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Creates a new book in the catalog.
    /// </summary>
    private static async Task<Results<CreatedAtRoute<Guid>, ProblemHttpResult>> CreateBook(
        [FromBody] CreateBookCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetBookById", new { id = result.Value })
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Updates an existing book's details.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBookCommand(new BookId(id), request.Title, request.Author, request.Description);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Deletes a book from the catalog.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteBook(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookCommand(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Seeds the catalog with sample book data from the provided JSON body.
    /// </summary>
    private static async Task<Results<Ok<SeedResult>, ProblemHttpResult>> SeedBooks(
        [FromBody] SeedBooksRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var items = request.Books
            .Select(b => new SeedBookItem(b.Title, b.Author, b.Description))
            .ToList();

        var result = await sender.Send(new SeedBooksCommand(items), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(new SeedResult(result.Value))
            : result.Error.ToProblemHttpResult();
    }
}

/// <summary>
/// Request body for updating a book.
/// </summary>
public sealed record UpdateBookRequest(string Title, string Author, string Description);

/// <summary>
/// Request body for seeding books.
/// </summary>
public sealed record SeedBooksRequest(IReadOnlyList<SeedBookRequest> Books);

/// <summary>
/// A single book in the seed request.
/// </summary>
public sealed record SeedBookRequest(string Title, string Author, string Description);

/// <summary>
/// Response from the seed endpoint.
/// </summary>
public sealed record SeedResult(int InsertedCount);
