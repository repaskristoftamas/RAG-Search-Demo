namespace BookStore.Gateway.HybridSearch;

/// <summary>
/// A single item in the fused hybrid search results, with transparency into per-source rankings.
/// </summary>
public sealed record HybridSearchResultItem(
    Guid BookId,
    string Title,
    string Author,
    string Description,
    double FusedScore,
    int? KeywordRank,
    int? SemanticRank);

/// <summary>
/// The full response from the hybrid search endpoint.
/// </summary>
public sealed record HybridSearchResponse(
    string Query,
    IReadOnlyList<HybridSearchResultItem> Results,
    int TotalCandidates,
    string? LlmSummary,
    long KeywordElapsedMs,
    long SemanticElapsedMs,
    long TotalElapsedMs);
