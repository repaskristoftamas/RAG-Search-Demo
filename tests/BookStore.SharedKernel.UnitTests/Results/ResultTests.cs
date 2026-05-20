using BookStore.SharedKernel.Results;
using Shouldly;

namespace BookStore.SharedKernel.UnitTests.Results;

/// <summary>
/// Tests for the non-generic <see cref="Result"/> type.
/// </summary>
public sealed class ResultTests
{
    [Fact]
    public void Success_IsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Success_IsFailureFalse()
    {
        var result = Result.Success();

        result.IsFailure.ShouldBeFalse();
    }

    [Fact]
    public void Failure_IsSuccessFalse()
    {
        var result = Result.Failure(new NotFoundError("NOT_FOUND", "Not found"));

        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void Failure_IsFailureTrue()
    {
        var result = Result.Failure(new NotFoundError("NOT_FOUND", "Not found"));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Failure_ErrorReturnsProvidedError()
    {
        var error = new NotFoundError("NOT_FOUND", "Resource not found");

        var result = Result.Failure(error);

        result.Error.ShouldBe(error);
    }

    [Fact]
    public void Success_AccessingErrorThrowsInvalidOperationException()
    {
        var result = Result.Success();

        Should.Throw<InvalidOperationException>(() => _ = result.Error);
    }
}
