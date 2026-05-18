using BookStore.KeywordSearch.Application.Search.DTOs;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.KeywordSearch.Application.Search.Queries.KeywordSearch;

/// <summary>
/// Query to perform a keyword-based full-text search over books.
/// </summary>
/// <param name="Query">The search terms.</param>
/// <param name="MaxResults">Maximum number of results to return.</param>
public sealed record KeywordSearchQuery(string Query, int MaxResults = 5) : IQuery<Result<IReadOnlyList<SearchResultDto>>>;
