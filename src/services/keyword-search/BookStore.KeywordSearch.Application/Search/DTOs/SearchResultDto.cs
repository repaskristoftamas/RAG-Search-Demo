namespace BookStore.KeywordSearch.Application.Search.DTOs;

/// <summary>
/// A single keyword search result with relevance score.
/// </summary>
/// <param name="BookId">The book identifier.</param>
/// <param name="Title">Title of the book.</param>
/// <param name="Author">Author name.</param>
/// <param name="Description">Short content description.</param>
/// <param name="RelevanceScore">PostgreSQL ts_rank score indicating match quality.</param>
public sealed record SearchResultDto(
    Guid BookId,
    string Title,
    string Author,
    string Description,
    double RelevanceScore);
