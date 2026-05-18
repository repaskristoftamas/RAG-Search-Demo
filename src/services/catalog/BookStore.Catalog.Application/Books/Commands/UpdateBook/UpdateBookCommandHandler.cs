using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using FluentValidation;
using Mediator;

namespace BookStore.Catalog.Application.Books.Commands.UpdateBook;

/// <summary>
/// Handles updating an existing book's details.
/// </summary>
internal sealed class UpdateBookCommandHandler(
    ICatalogDbContext context,
    IValidator<UpdateBookCommand> validator) : ICommandHandler<UpdateBookCommand, Result>
{
    /// <summary>
    /// Updates the book with the specified identifier.
    /// </summary>
    public async ValueTask<Result> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var failures = validationResult.Errors
                .Select(f => new FieldValidationFailure(f.PropertyName, f.ErrorCode, f.ErrorMessage))
                .ToList();
            return Result.Failure(new ValidationError(failures));
        }

        var book = await context.Books.FindAsync([command.Id], cancellationToken);
        if (book is null)
            return Result.Failure(new NotFoundError(BookErrorCodes.NotFound, "The book with the specified identifier was not found."));

        var updateResult = book.Update(command.Title, command.Author, command.Description);
        if (updateResult.IsFailure)
            return updateResult;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
