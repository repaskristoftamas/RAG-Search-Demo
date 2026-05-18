using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Gateway.Comparison;

/// <summary>
/// Defines the comparison endpoint that calls both search services in parallel.
/// </summary>
public static class ComparisonEndpoints
{
    /// <summary>
    /// Registers the comparison route.
    /// </summary>
    public static void MapComparisonEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/compare").WithTags("Comparison");

        group.MapGet("/", Compare).WithName("CompareSearch");
    }

    /// <summary>
    /// Calls both keyword and semantic search in parallel and returns side-by-side results with timing.
    /// </summary>
    private static async Task<Results<Ok<ComparisonResponse>, ProblemHttpResult>> Compare(
        [FromQuery] string q,
        SearchServiceClient searchClient,
        CancellationToken cancellationToken,
        [FromQuery] int maxResults = 10)
    {
        var totalStopwatch = Stopwatch.StartNew();

        var keywordTask = searchClient.SearchKeywordAsync(q, maxResults, cancellationToken);
        var semanticTask = searchClient.SearchSemanticAsync(q, maxResults, cancellationToken);

        try
        {
            await Task.WhenAll(keywordTask, semanticTask);
        }
        catch (HttpRequestException)
        {
            return TypedResults.Problem(
                statusCode: 502,
                title: "One or more search services are unavailable");
        }

        totalStopwatch.Stop();

        var response = new ComparisonResponse(
            q,
            keywordTask.Result,
            semanticTask.Result,
            totalStopwatch.ElapsedMilliseconds);

        return TypedResults.Ok(response);
    }
}
