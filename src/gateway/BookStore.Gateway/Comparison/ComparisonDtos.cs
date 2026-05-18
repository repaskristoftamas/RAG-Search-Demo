namespace BookStore.Gateway.Comparison;

/// <summary>
/// A single search result from either search service.
/// </summary>
public sealed record SearchResultItem(
    Guid BookId,
    string Title,
    string Author,
    string Description,
    double Score);

/// <summary>
/// Results from a single search service, including timing.
/// </summary>
public sealed record SearchServiceResponse(
    IReadOnlyList<SearchResultItem> Results,
    long ElapsedMs,
    string? LlmSummary = null);

/// <summary>
/// Side-by-side comparison of keyword and semantic search results.
/// </summary>
public sealed record ComparisonResponse(
    string Query,
    SearchServiceResponse KeywordResults,
    SearchServiceResponse SemanticResults,
    long TotalElapsedMs);
