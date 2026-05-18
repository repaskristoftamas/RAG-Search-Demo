using BookStore.SemanticSearch.Application.Search.DTOs;
using BookStore.SemanticSearch.Application.Search.Queries.SemanticSearch;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.SemanticSearch.Api.Endpoints;

/// <summary>
/// Defines the semantic search endpoint.
/// </summary>
public static class SearchEndpoints
{
    /// <summary>
    /// Registers the search route.
    /// </summary>
    public static void MapSearchEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/search").WithTags("SemanticSearch");

        group.MapGet("/", Search).WithName("SemanticSearch");
    }

    /// <summary>
    /// Performs a semantic vector-based search with optional LLM summary.
    /// </summary>
    private static async Task<Results<Ok<SemanticSearchResponse>, ProblemHttpResult>> Search(
        [FromQuery] string q,
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int maxResults = 5,
        [FromQuery] bool includeLlmSummary = false)
    {
        var result = await sender.Send(
            new SemanticSearchQuery(q, maxResults, includeLlmSummary), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(statusCode: 500, title: "Search failed");
    }
}
