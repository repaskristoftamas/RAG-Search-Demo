using BookStore.Catalog.Application.Books.Commands.CreateBook;
using BookStore.Catalog.Domain.Books;
using Shouldly;

namespace BookStore.Catalog.Application.UnitTests.Books.Commands;

public sealed class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateBookCommand("Title", "Author", "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingTitle_HasTitleRequiredError(string? title)
    {
        var command = new CreateBookCommand(title!, "Author", "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.TitleRequired);
    }

    [Fact]
    public void Validate_TitleExceedsMaxLength_HasTitleTooLongError()
    {
        var command = new CreateBookCommand(new string('x', 501), "Author", "Description");

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
        var command = new CreateBookCommand("Title", author!, "Description");

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.AuthorRequired);
    }

    [Fact]
    public void Validate_AuthorExceedsMaxLength_HasAuthorTooLongError()
    {
        var command = new CreateBookCommand("Title", new string('x', 251), "Description");

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
        var command = new CreateBookCommand("Title", "Author", description!);

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorCode == BookErrorCodes.DescriptionRequired);
    }
}
