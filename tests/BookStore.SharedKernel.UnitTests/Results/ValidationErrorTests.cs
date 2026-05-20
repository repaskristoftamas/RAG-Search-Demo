using BookStore.SharedKernel.Results;
using Shouldly;

namespace BookStore.SharedKernel.UnitTests.Results;

/// <summary>
/// Tests for <see cref="ValidationError"/> description aggregation.
/// </summary>
public sealed class ValidationErrorTests
{
    [Fact]
    public void Code_AlwaysReturnsValidationFailed()
    {
        var error = new ValidationError([new FieldValidationFailure("Name", "REQUIRED", "Name is required")]);

        error.Code.ShouldBe("VALIDATION_FAILED");
    }

    [Fact]
    public void Description_SingleFailure_ContainsFailureDescription()
    {
        var error = new ValidationError([new FieldValidationFailure("Name", "REQUIRED", "Name is required")]);

        error.Description.ShouldBe("Name is required");
    }

    [Fact]
    public void Description_MultipleFailures_JoinsWithSemicolon()
    {
        var failures = new List<FieldValidationFailure>
        {
            new("Title", "REQUIRED", "Title is required"),
            new("Author", "REQUIRED", "Author is required")
        };

        var error = new ValidationError(failures);

        error.Description.ShouldBe("Title is required; Author is required");
    }

    [Fact]
    public void Failures_ReturnsProvidedFailures()
    {
        var failures = new List<FieldValidationFailure>
        {
            new("Title", "REQUIRED", "Title is required"),
            new("Price", "RANGE", "Price must be positive")
        };

        var error = new ValidationError(failures);

        error.Failures.Count.ShouldBe(2);
        error.Failures[0].PropertyName.ShouldBe("Title");
        error.Failures[1].PropertyName.ShouldBe("Price");
    }
}
