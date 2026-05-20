using BookStore.Gateway.Comparison;

namespace BookStore.Gateway.HybridSearch;

/// <summary>
/// Fuses ranked result lists using Reciprocal Rank Fusion (Cormack et al., 2009).
/// </summary>
public sealed class ReciprocalRankFusionStrategy : IRankFusionStrategy
{
    /// <summary>
    /// Smoothing constant from the original RRF paper. Dampens rank differences so that
    /// rank 1 vs rank 2 is not a disproportionately large gap.
    /// </summary>
    private const int K = 60;

    /// <inheritdoc />
    public IReadOnlyList<HybridSearchResultItem> Fuse(
        IReadOnlyList<SearchResultItem> keywordResults,
        IReadOnlyList<SearchResultItem> semanticResults,
        int maxResults,
        double keywordWeight,
        double semanticWeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keywordWeight);
        ArgumentOutOfRangeException.ThrowIfNegative(semanticWeight);

        if (keywordWeight == 0 && semanticWeight == 0)
        {
            throw new ArgumentException("At least one weight must be greater than zero.");
        }

        var totalWeight = keywordWeight + semanticWeight;
        var normalizedKeywordWeight = keywordWeight / totalWeight;
        var normalizedSemanticWeight = semanticWeight / totalWeight;

        var candidates = new Dictionary<Guid, CandidateBuilder>();

        for (var rank = 0; rank < keywordResults.Count; rank++)
        {
            var item = keywordResults[rank];
            var builder = GetOrAddCandidate(candidates, item);
            builder.KeywordRank = rank + 1;
            builder.FusedScore += normalizedKeywordWeight * (1.0 / (K + rank + 1));
        }

        for (var rank = 0; rank < semanticResults.Count; rank++)
        {
            var item = semanticResults[rank];
            var builder = GetOrAddCandidate(candidates, item);
            builder.SemanticRank = rank + 1;
            builder.FusedScore += normalizedSemanticWeight * (1.0 / (K + rank + 1));
        }

        return [.. candidates.Values
            .OrderByDescending(c => c.FusedScore)
            .Take(maxResults)
            .Select(c => new HybridSearchResultItem(
                c.BookId, c.Title, c.Author, c.Description,
                c.FusedScore, c.KeywordRank, c.SemanticRank))];
    }

    /// <summary>
    /// Gets an existing candidate or creates a new one from the search result item.
    /// </summary>
    private static CandidateBuilder GetOrAddCandidate(
        Dictionary<Guid, CandidateBuilder> candidates, SearchResultItem item)
    {
        if (!candidates.TryGetValue(item.BookId, out var builder))
        {
            builder = new CandidateBuilder
            {
                BookId = item.BookId,
                Title = item.Title,
                Author = item.Author,
                Description = item.Description
            };
            candidates[item.BookId] = builder;
        }

        return builder;
    }

    /// <summary>
    /// Mutable accumulator used during fusion to build up scores and rank positions.
    /// </summary>
    private sealed class CandidateBuilder
    {
        public Guid BookId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double FusedScore { get; set; }
        public int? KeywordRank { get; set; }
        public int? SemanticRank { get; set; }
    }
}
