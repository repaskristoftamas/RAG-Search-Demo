namespace BookStore.Catalog.Domain.Books;

/// <summary>
/// Machine-readable error codes for Book domain operations.
/// </summary>
public static class BookErrorCodes
{
    /// <summary>The requested book does not exist.</summary>
    public const string NotFound = "BOOK_NOT_FOUND";

    /// <summary>Title is missing or whitespace.</summary>
    public const string TitleRequired = "BOOK_TITLE_REQUIRED";

    /// <summary>Title exceeds the maximum allowed length.</summary>
    public const string TitleTooLong = "BOOK_TITLE_TOO_LONG";

    /// <summary>Author is missing or whitespace.</summary>
    public const string AuthorRequired = "BOOK_AUTHOR_REQUIRED";

    /// <summary>Author exceeds the maximum allowed length.</summary>
    public const string AuthorTooLong = "BOOK_AUTHOR_TOO_LONG";

    /// <summary>Description is missing or whitespace.</summary>
    public const string DescriptionRequired = "BOOK_DESCRIPTION_REQUIRED";
}
