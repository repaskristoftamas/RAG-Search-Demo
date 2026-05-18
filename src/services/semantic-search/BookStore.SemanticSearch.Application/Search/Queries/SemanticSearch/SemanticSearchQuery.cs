using BookStore.SemanticSearch.Application.Search.DTOs;
using BookStore.SharedKernel.Results;
using Mediator;

namespace BookStore.SemanticSearch.Application.Search.Queries.SemanticSearch;

/// <summary>
/// Query to perform a semantic vector-based search over books.
/// </summary>
/// <param name="Query">The natural language search query.</param>
/// <param name="MaxResults">Maximum number of results to return.</param>
/// <param name="IncludeLlmSummary">Whether to include an LLM-generated summary of the results.</param>
public sealed record SemanticSearchQuery(
    string Query,
    int MaxResults = 5,
    bool IncludeLlmSummary = false) : IQuery<Result<SemanticSearchResponse>>;
