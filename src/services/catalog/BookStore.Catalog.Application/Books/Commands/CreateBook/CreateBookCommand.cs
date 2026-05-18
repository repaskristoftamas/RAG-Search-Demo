using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.CreateBook;

/// <summary>
/// Command to add a new book to the catalog, returning the generated identifier on success.
/// </summary>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Author name.</param>
/// <param name="Description">Short content description of the book.</param>
public sealed record CreateBookCommand(
    string Title,
    string Author,
    string Description) : ICommand<Result<Guid>>;
