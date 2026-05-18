using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.SeedBooks;

/// <summary>
/// Command to seed the catalog with sample book data.
/// </summary>
/// <param name="Books">The collection of books to seed.</param>
public sealed record SeedBooksCommand(IReadOnlyList<SeedBookItem> Books) : ICommand<Result<int>>;

/// <summary>
/// A single book item in the seed data.
/// </summary>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Author name.</param>
/// <param name="Description">Short content description of the book.</param>
public sealed record SeedBookItem(string Title, string Author, string Description);
