using BookStore.Gateway.Comparison;

namespace BookStore.Gateway.HybridSearch;

/// <summary>
/// Defines a strategy for fusing multiple ranked search result lists into a single ranked list.
/// </summary>
public interface IRankFusionStrategy
{
    /// <summary>
    /// Fuses keyword and semantic search results into a single ranked list.
    /// </summary>
    /// <param name="keywordResults">Ranked results from keyword search (order matters).</param>
    /// <param name="semanticResults">Ranked results from semantic search (order matters).</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="keywordWeight">Relative weight for keyword search contributions.</param>
    /// <param name="semanticWeight">Relative weight for semantic search contributions.</param>
    /// <returns>A fused and ranked list of <see cref="HybridSearchResultItem"/>.</returns>
    IReadOnlyList<HybridSearchResultItem> Fuse(
        IReadOnlyList<SearchResultItem> keywordResults,
        IReadOnlyList<SearchResultItem> semanticResults,
        int maxResults,
        double keywordWeight,
        double semanticWeight);
}
