using BookStore.Catalog.Domain.Books.Events;
using BookStore.SharedKernel.Abstractions;
using BookStore.SharedKernel.Results;

namespace BookStore.Catalog.Domain.Books;

/// <summary>
/// Domain entity representing a book in the catalog.
/// </summary>
public sealed class Book : AuditableEntity<BookId>
{
    /// <summary>
    /// Title of the book.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Author name.
    /// </summary>
    public string Author { get; private set; } = string.Empty;

    /// <summary>
    /// Short content description of the book.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Factory method that creates a new book with a generated identifier.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="author">Author name.</param>
    /// <param name="description">Short content description.</param>
    /// <returns>A <see cref="Result{Book}"/> containing the new instance on success, or a validation error on failure.</returns>
    public static Result<Book> Create(string title, string author, string description)
    {
        var validation = Validate(title, author, description);
        if (validation.IsFailure)
            return Result.Failure<Book>(validation.Error);

        var book = new Book
        {
            Id = BookId.New(),
            Title = title,
            Author = author,
            Description = description
        };

        book.AddDomainEvent(new BookCreatedEvent(book.Id));

        return Result.Success(book);
    }

    /// <summary>
    /// Replaces all mutable properties of the book with the provided values.
    /// </summary>
    /// <param name="title">Title of the book.</param>
    /// <param name="author">Author name.</param>
    /// <param name="description">Short content description.</param>
    /// <returns>A success result if all values are valid, or a validation error otherwise.</returns>
    public Result Update(string title, string author, string description)
    {
        var validation = Validate(title, author, description);
        if (validation.IsFailure)
            return validation;

        Title = title;
        Author = author;
        Description = description;

        AddDomainEvent(new BookUpdatedEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks this book for deletion and raises the <see cref="BookDeletedEvent"/>.
    /// </summary>
    public void Delete() => AddDomainEvent(new BookDeletedEvent(Id));

    /// <summary>
    /// Last-resort invariant guard that protects structural integrity regardless of entry point.
    /// </summary>
    private static Result Validate(string title, string author, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Title), BookErrorCodes.TitleRequired, "Title is required.")]));

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Author), BookErrorCodes.AuthorRequired, "Author is required.")]));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(new ValidationError([new FieldValidationFailure(nameof(Description), BookErrorCodes.DescriptionRequired, "Description is required.")]));

        return Result.Success();
    }
}
