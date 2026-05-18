using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.UpdateBook;

/// <summary>
/// Command to update an existing book's details.
/// </summary>
/// <param name="Id">The identifier of the book to update.</param>
/// <param name="Title">Updated title.</param>
/// <param name="Author">Updated author name.</param>
/// <param name="Description">Updated description.</param>
public sealed record UpdateBookCommand(
    BookId Id,
    string Title,
    string Author,
    string Description) : ICommand<Result>;
