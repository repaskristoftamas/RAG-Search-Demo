using BookStore.KeywordSearch.Application.Search.DTOs;
using BookStore.KeywordSearch.Application.Search.Queries.KeywordSearch;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.KeywordSearch.Api.Endpoints;

/// <summary>
/// Defines the keyword search endpoint.
/// </summary>
public static class SearchEndpoints
{
    /// <summary>
    /// Registers the search route.
    /// </summary>
    public static void MapSearchEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/search").WithTags("KeywordSearch");

        group.MapGet("/", Search).WithName("KeywordSearch");
    }

    /// <summary>
    /// Performs a keyword-based full-text search.
    /// </summary>
    private static async Task<Results<Ok<IReadOnlyList<SearchResultDto>>, ProblemHttpResult>> Search(
        [FromQuery] string q,
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int maxResults = 5)
    {
        var result = await sender.Send(new KeywordSearchQuery(q, maxResults), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(statusCode: 500, title: "Search failed");
    }
}
