using BookStore.SharedKernel.Results;
using Shouldly;

namespace BookStore.SharedKernel.UnitTests.Results;

/// <summary>
/// Tests for the generic <see cref="Result{TValue}"/> type.
/// </summary>
public sealed class GenericResultTests
{
    [Fact]
    public void Success_ValueReturnsProvidedValue()
    {
        var result = Result.Success(42);

        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Success_IsSuccessTrue()
    {
        var result = Result.Success("hello");

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Failure_IsFailureTrue()
    {
        var result = Result.Failure<int>(new NotFoundError("NOT_FOUND", "Not found"));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Failure_AccessingValueThrowsInvalidOperationException()
    {
        var result = Result.Failure<int>(new NotFoundError("NOT_FOUND", "Not found"));

        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Failure_ErrorReturnsProvidedError()
    {
        var error = new ConflictError("CONFLICT", "Already exists");

        var result = Result.Failure<string>(error);

        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Success_AccessingErrorThrowsInvalidOperationException()
    {
        var result = Result.Success(42);

        Should.Throw<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void ImplicitConversion_WrapsValueInSuccessResult()
    {
        Result<int> result = 99;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(99);
    }

    [Fact]
    public void Success_WithReferenceType_ReturnsValue()
    {
        var result = Result.Success<string?>("test");

        result.Value.ShouldBe("test");
    }
}
