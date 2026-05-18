using BookStore.Catalog.Domain.Books;
using FluentValidation;

namespace BookStore.Catalog.Application.Books.Commands.CreateBook;

/// <summary>
/// Validates a <see cref="CreateBookCommand"/> before it reaches the handler.
/// </summary>
public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    /// <summary>Initializes the validation rules.</summary>
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode(BookErrorCodes.TitleRequired)
            .MaximumLength(500).WithErrorCode(BookErrorCodes.TitleTooLong);

        RuleFor(x => x.Author)
            .NotEmpty().WithErrorCode(BookErrorCodes.AuthorRequired)
            .MaximumLength(250).WithErrorCode(BookErrorCodes.AuthorTooLong);

        RuleFor(x => x.Description)
            .NotEmpty().WithErrorCode(BookErrorCodes.DescriptionRequired);
    }
}
