using System.Diagnostics;
using BookStore.Gateway.Comparison;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Gateway.HybridSearch;

/// <summary>
/// Defines the hybrid search endpoint that fuses keyword and semantic results using rank fusion.
/// </summary>
public static class HybridSearchEndpoints
{
    /// <summary>
    /// Multiplier applied to maxResults when fetching from each backend.
    /// RRF can only fuse what it sees — over-fetching ensures we don't miss high-fusing candidates.
    /// </summary>
    private const int OverFetchMultiplier = 3;

    /// <summary>
    /// Registers the hybrid search route.
    /// </summary>
    public static void MapHybridSearchEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/search").WithTags("HybridSearch");

        group.MapGet("/", Search).WithName("HybridSearch");
    }

    /// <summary>
    /// Calls both search services in parallel, fuses results with the configured rank fusion strategy,
    /// and returns a single ranked list.
    /// </summary>
    private static async Task<Results<Ok<HybridSearchResponse>, ValidationProblem, ProblemHttpResult>> Search(
        [FromQuery] string q,
        SearchServiceClient searchClient,
        IRankFusionStrategy fusionStrategy,
        CancellationToken cancellationToken,
        [FromQuery] int maxResults = 10,
        [FromQuery] double keywordWeight = 0.5,
        [FromQuery] double semanticWeight = 0.5)
    {
        var errors = new Dictionary<string, string[]>();

        if (keywordWeight < 0)
        {
            errors[nameof(keywordWeight)] = ["keywordWeight must not be negative."];
        }

        if (semanticWeight < 0)
        {
            errors[nameof(semanticWeight)] = ["semanticWeight must not be negative."];
        }

        if (keywordWeight == 0 && semanticWeight == 0)
        {
            errors[nameof(keywordWeight)] = ["At least one weight must be greater than zero."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var totalStopwatch = Stopwatch.StartNew();
        var fetchCount = maxResults * OverFetchMultiplier;

        var keywordTask = searchClient.SearchKeywordAsync(q, fetchCount, cancellationToken);
        var semanticTask = searchClient.SearchSemanticAsync(q, fetchCount, cancellationToken);

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

        var keywordResponse = await keywordTask;
        var semanticResponse = await semanticTask;

        var fused = fusionStrategy.Fuse(
            keywordResponse.Results,
            semanticResponse.Results,
            maxResults,
            keywordWeight,
            semanticWeight);

        totalStopwatch.Stop();

        var totalCandidates = keywordResponse.Results
            .Select(r => r.BookId)
            .Concat(semanticResponse.Results.Select(r => r.BookId))
            .Distinct()
            .Count();

        var response = new HybridSearchResponse(
            q,
            fused,
            totalCandidates,
            semanticResponse.LlmSummary,
            keywordResponse.ElapsedMs,
            semanticResponse.ElapsedMs,
            totalStopwatch.ElapsedMilliseconds);

        return TypedResults.Ok(response);
    }
}
