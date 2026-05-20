using BookStore.Catalog.Application.Books.Commands.UpdateBook;
using BookStore.Catalog.Domain.Books;
using Shouldly;

namespace BookStore.Catalog.Application.UnitTests.Books.Commands;

public sealed class UpdateBookCommandValidatorTests
{
    private readonly UpdateBookCommandValidator _validator = new();
    private static readonly BookId SomeBookId = BookId.New();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new UpdateBookCommand(SomeBookId, "Title", "Author", "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingTitle_HasTitleRequiredError(string? title)
    {
        var command = new UpdateBookCommand(SomeBookId, title!, "Author", "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.TitleRequired);
    }

    [Fact]
    public void Validate_TitleExceedsMaxLength_HasTitleTooLongError()
    {
        var command = new UpdateBookCommand(SomeBookId, new string('x', 501), "Author", "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.TitleTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingAuthor_HasAuthorRequiredError(string? author)
    {
        var command = new UpdateBookCommand(SomeBookId, "Title", author!, "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.AuthorRequired);
    }

    [Fact]
    public void Validate_AuthorExceedsMaxLength_HasAuthorTooLongError()
    {
        var command = new UpdateBookCommand(SomeBookId, "Title", new string('x', 251), "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.AuthorTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingDescription_HasDescriptionRequiredError(string? description)
    {
        var command = new UpdateBookCommand(SomeBookId, "Title", "Author", description!);

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.DescriptionRequired);
    }
}
