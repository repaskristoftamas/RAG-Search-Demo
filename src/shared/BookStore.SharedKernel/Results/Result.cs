namespace BookStore.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail, without a return value.
/// </summary>
public class Result
{
    private readonly Error? _error;

    /// <summary>
    /// Initializes a new result.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error describing the failure, or <c>null</c> for a success.</param>
    /// <exception cref="InvalidOperationException">Thrown when success/error state is inconsistent.</exception>
    private protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException();
        if (!isSuccess && error is null)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error describing why the operation failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a successful result.</exception>
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("The error of a success result cannot be accessed.");

    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a successful result carrying the specified value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result of the specified value type with the given error.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the outcome of an operation that can succeed with a value or fail with an error.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new result with an optional value.
    /// </summary>
    internal Result(TValue? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The value produced by a successful operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    /// <summary>
    /// Implicitly wraps a value in a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
