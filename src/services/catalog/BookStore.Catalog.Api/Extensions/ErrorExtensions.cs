using BookStore.SharedKernel.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookStore.Catalog.Api.Extensions;

/// <summary>
/// Extension methods for converting domain errors to HTTP problem responses.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Converts an <see cref="Error"/> to a <see cref="ProblemHttpResult"/> with the appropriate HTTP status code.
    /// </summary>
    public static ProblemHttpResult ToProblemHttpResult(this Error error)
    {
        var (statusCode, title) = error switch
        {
            NotFoundError   => (StatusCodes.Status404NotFound, "NotFound"),
            ConflictError   => (StatusCodes.Status409Conflict, "Conflict"),
            ValidationError => (StatusCodes.Status400BadRequest, "ValidationError"),
            _               => (StatusCodes.Status500InternalServerError, "InternalError")
        };

        var extensions = error is ValidationError validationError
            ? new Dictionary<string, object?>(2) { ["errorCode"] = error.Code, ["failures"] = validationError.Failures }
            : new Dictionary<string, object?>(1) { ["errorCode"] = error.Code };

        return TypedResults.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Description,
            extensions: extensions);
    }
}
