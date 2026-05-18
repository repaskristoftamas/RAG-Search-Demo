using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Search.DTOs;
using BookStore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace BookStore.SemanticSearch.Application.Search.Queries.SemanticSearch;

/// <summary>
/// Handles semantic search using pgvector cosine similarity, with optional LLM summary generation.
/// </summary>
internal sealed class SemanticSearchQueryHandler(
    ISemanticSearchDbContext context,
    IEmbeddingService embeddingService,
    ITextGenerationService textGenerationService) : IQueryHandler<SemanticSearchQuery, Result<SemanticSearchResponse>>
{
    /// <summary>
    /// Executes a semantic search by embedding the query and finding the closest vectors.
    /// </summary>
    public async ValueTask<Result<SemanticSearchResponse>> Handle(
        SemanticSearchQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
            return Result.Success(new SemanticSearchResponse([], null));

        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query.Query, cancellationToken);
        var queryVector = new Vector(queryEmbedding);

        const double MinSimilarity = 0.5;

        var results = await context.BookEmbeddings
            .Where(b => b.Embedding.CosineDistance(queryVector) < 1 - MinSimilarity)
            .OrderBy(b => b.Embedding.CosineDistance(queryVector))
            .Take(query.MaxResults)
            .Select(b => new SemanticSearchResultDto(
                b.Id,
                b.Title,
                b.Author,
                b.Description,
                1 - b.Embedding.CosineDistance(queryVector)))
            .ToListAsync(cancellationToken);

        string? llmSummary = null;

        if (query.IncludeLlmSummary && results.Count > 0)
        {
            var contextText = string.Join("\n\n", results.Select(r =>
                $"Title: {r.Title}\nAuthor: {r.Author}\nDescription: {r.Description}"));

            var prompt = $"""
                Based on the following book search results for the query "{query.Query}",
                provide a brief summary of the most relevant books and why they match the query.

                Search Results:
                {contextText}

                Summary:
                """;

            llmSummary = await textGenerationService.GenerateAsync(prompt, cancellationToken);
        }

        return Result.Success(new SemanticSearchResponse(results, llmSummary));
    }
}
