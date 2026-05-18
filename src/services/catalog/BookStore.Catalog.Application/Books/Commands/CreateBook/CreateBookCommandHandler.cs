using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Application.Extensions;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using FluentValidation;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.CreateBook;

/// <summary>
/// Handles creation of a new book in the catalog.
/// </summary>
internal sealed class CreateBookCommandHandler(
    ICatalogDbContext context,
    IValidator<CreateBookCommand> validator) : ICommandHandler<CreateBookCommand, Result<Guid>>
{
    /// <summary>
    /// Creates a new book and returns its identifier.
    /// </summary>
    public async ValueTask<Result<Guid>> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailureResult<Guid>();

        var createResult = Book.Create(command.Title, command.Author, command.Description);
        if (createResult.IsFailure)
            return Result.Failure<Guid>(createResult.Error);

        var book = createResult.Value;
        context.Books.Add(book);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(book.Id.Value);
    }
}
