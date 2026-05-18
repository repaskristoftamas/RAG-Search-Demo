using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.DeleteBook;

/// <summary>
/// Command to delete a book from the catalog.
/// </summary>
/// <param name="Id">The identifier of the book to delete.</param>
public sealed record DeleteBookCommand(BookId Id) : ICommand<Result>;
