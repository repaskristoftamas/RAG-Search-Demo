using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookStore.Gateway.Comparison;

/// <summary>
/// Typed HTTP client that calls the keyword and semantic search services.
/// </summary>
public sealed class SearchServiceClient(
    IHttpClientFactory httpClientFactory,
    ILogger<SearchServiceClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Calls the keyword search service and returns normalized results with timing.
    /// </summary>
    public async Task<SearchServiceResponse> SearchKeywordAsync(
        string query, int maxResults, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("KeywordSearch");
        var stopwatch = Stopwatch.StartNew();

        var results = await client.GetFromJsonAsync<List<KeywordSearchResult>>(
            $"/api/v1/search?q={Uri.EscapeDataString(query)}&maxResults={maxResults}",
            JsonOptions,
            cancellationToken);

        stopwatch.Stop();

        var items = (results ?? [])
            .Select(r => new SearchResultItem(r.BookId, r.Title, r.Author, r.Description, r.RelevanceScore))
            .ToList();

        logger.LogInformation("Keyword search for '{Query}' returned {Count} results in {ElapsedMs}ms",
            query, items.Count, stopwatch.ElapsedMilliseconds);

        return new SearchServiceResponse(items, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Calls the semantic search service and returns normalized results with timing.
    /// </summary>
    public async Task<SearchServiceResponse> SearchSemanticAsync(
        string query, int maxResults, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("SemanticSearch");
        var stopwatch = Stopwatch.StartNew();

        var response = await client.GetFromJsonAsync<SemanticSearchApiResponse>(
            $"/api/v1/search?q={Uri.EscapeDataString(query)}&maxResults={maxResults}&includeLlmSummary=true",
            JsonOptions,
            cancellationToken);

        stopwatch.Stop();

        var items = (response?.Results ?? [])
            .Select(r => new SearchResultItem(r.BookId, r.Title, r.Author, r.Description, r.SimilarityScore))
            .ToList();

        logger.LogInformation("Semantic search for '{Query}' returned {Count} results in {ElapsedMs}ms",
            query, items.Count, stopwatch.ElapsedMilliseconds);

        return new SearchServiceResponse(items, stopwatch.ElapsedMilliseconds, response?.LlmSummary);
    }

    private sealed record KeywordSearchResult(
        Guid BookId, string Title, string Author, string Description, double RelevanceScore);

    private sealed record SemanticSearchApiResponse(
        IReadOnlyList<SemanticSearchItem> Results, string? LlmSummary);

    private sealed record SemanticSearchItem(
        Guid BookId, string Title, string Author, string Description, double SimilarityScore);
}
