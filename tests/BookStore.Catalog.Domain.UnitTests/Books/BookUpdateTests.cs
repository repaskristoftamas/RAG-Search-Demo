using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.Domain.Books.Events;
using BookStore.SharedKernel.Results;

namespace BookStore.Catalog.Domain.UnitTests.Books;

public sealed class BookUpdateTests
{
    private const string ValidTitle = "Clean Architecture";
    private const string ValidAuthor = "Robert C. Martin";
    private const string ValidDescription = "A comprehensive guide to software architecture.";
    private const string UpdatedTitle = "Clean Code";
    private const string UpdatedAuthor = "Uncle Bob";
    private const string UpdatedDescription = "A handbook of agile software craftsmanship.";

    /// <summary>
    /// Creates a valid book and clears the initial domain events so each test starts clean.
    /// </summary>
    private static Book CreateValidBook()
    {
        var book = Book.Create(ValidTitle, ValidAuthor, ValidDescription).Value;
        book.ClearDomainEvents();
        return book;
    }

    [Fact]
    public void Update_ValidInputs_ReturnsSuccessResult()
    {
        var book = CreateValidBook();

        var result = book.Update(UpdatedTitle, UpdatedAuthor, UpdatedDescription);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Update_ValidInputs_MutatesProperties()
    {
        var book = CreateValidBook();

        book.Update(UpdatedTitle, UpdatedAuthor, UpdatedDescription);

        book.Title.ShouldBe(UpdatedTitle);
        book.Author.ShouldBe(UpdatedAuthor);
        book.Description.ShouldBe(UpdatedDescription);
    }

    [Fact]
    public void Update_ValidInputs_EmitsBookUpdatedEvent()
    {
        var book = CreateValidBook();

        book.Update(UpdatedTitle, UpdatedAuthor, UpdatedDescription);

        book.DomainEvents.ShouldHaveSingleItem();
        var domainEvent = book.DomainEvents[0].ShouldBeOfType<BookUpdatedEvent>();
        domainEvent.BookId.ShouldBe(book.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_MissingTitle_ReturnsValidationErrorWithTitleRequiredCode(string? title)
    {
        var book = CreateValidBook();

        var result = book.Update(title!, ValidAuthor, ValidDescription);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldHaveSingleItem()
            .Code.ShouldBe(BookErrorCodes.TitleRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_MissingAuthor_ReturnsValidationErrorWithAuthorRequiredCode(string? author)
    {
        var book = CreateValidBook();

        var result = book.Update(ValidTitle, author!, ValidDescription);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldHaveSingleItem()
            .Code.ShouldBe(BookErrorCodes.AuthorRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_MissingDescription_ReturnsValidationErrorWithDescriptionRequiredCode(string? description)
    {
        var book = CreateValidBook();

        var result = book.Update(ValidTitle, ValidAuthor, description!);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Failures.ShouldHaveSingleItem()
            .Code.ShouldBe(BookErrorCodes.DescriptionRequired);
    }

    [Fact]
    public void Update_ValidationFailure_DoesNotMutateState()
    {
        var book = CreateValidBook();

        book.Update("", UpdatedAuthor, UpdatedDescription);

        book.Title.ShouldBe(ValidTitle);
        book.Author.ShouldBe(ValidAuthor);
        book.Description.ShouldBe(ValidDescription);
    }

    [Fact]
    public void Update_ValidationFailure_DoesNotEmitEvent()
    {
        var book = CreateValidBook();

        book.Update("", UpdatedAuthor, UpdatedDescription);

        book.DomainEvents.ShouldBeEmpty();
    }
}
