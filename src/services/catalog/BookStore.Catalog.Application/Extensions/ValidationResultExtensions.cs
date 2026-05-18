using BookStore.SharedKernel.Results;
using FluentValidation.Results;

namespace BookStore.Catalog.Application.Extensions;

/// <summary>
/// Extension methods for converting <see cref="ValidationResult"/> into the Result pattern.
/// </summary>
internal static class ValidationResultExtensions
{
    /// <summary>
    /// Converts a failed <see cref="ValidationResult"/> into a <see cref="Result{T}"/> failure.
    /// </summary>
    internal static Result<T> ToFailureResult<T>(this ValidationResult validationResult) =>
        Result.Failure<T>(new ValidationError(
            [.. validationResult.Errors.Select(f => new FieldValidationFailure(f.PropertyName, f.ErrorCode, f.ErrorMessage))]));
}
