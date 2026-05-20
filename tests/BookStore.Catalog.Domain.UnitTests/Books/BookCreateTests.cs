using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;
using BookStore.SharedKernel.Results;

namespace BookStore.Catalog.Domain.UnitTests.Books;

public sealed class BookCreateTests
{
    private const string ValidTitle = "Clean Architecture";
    private const string ValidAuthor = "Robert C. Martin";
    private const string ValidDescription = "A comprehensive guide to software architecture.";

    [Fact]
    public void Create_ValidInputs_ReturnsSuccessResult()
    {
        var result = Book.Create(ValidTitle, ValidAuthor, ValidDescription);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Create_ValidInputs_SetsAllProperties()
    {
        var result = Book.Create(ValidTitle, ValidAuthor, ValidDescription);

        var book = result.Value;
        book.Title.ShouldBe(ValidTitle);
        book.Author.ShouldBe(ValidAuthor);
        book.Description.ShouldBe(ValidDescription);
    }

    [Fact]
    public void Create_ValidInputs_GeneratesNonDefaultId()
    {
        var result = Book.Create(ValidTitle, ValidAuthor, ValidDescription);

        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ValidInputs_EmitsBookCreatedEvent()
    {
        var result = Book.Create(ValidTitle, ValidAuthor, ValidDescription);

        var book = result.Value;
        book.DomainEvents.ShouldHaveSingleItem();
        var domainEvent = book.DomainEvents[0].ShouldBeOfType<BookCreatedEvent>();
        domainEvent.BookId.ShouldBe(book.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingTitle_ReturnsValidationErrorWithTitleRequiredCode(string? title)
    {
        var result = Book.Create(title!, ValidAuthor, ValidDescription);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldHaveSingleItem()
            .Code.ShouldBe(BookErrorCodes.TitleRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingAuthor_ReturnsValidationErrorWithAuthorRequiredCode(string? author)
    {
        var result = Book.Create(ValidTitle, author!, ValidDescription);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldHaveSingleItem()
            .Code.ShouldBe(BookErrorCodes.AuthorRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingDescription_ReturnsValidationErrorWithDescriptionRequiredCode(string? description)
    {
        var result = Book.Create(ValidTitle, ValidAuthor, description!);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldHaveSingleItem()
            .Code.ShouldBe(BookErrorCodes.DescriptionRequired);
    }
}
