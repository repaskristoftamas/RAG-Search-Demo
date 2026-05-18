using BookStore.KeywordSearch.Application.Abstractions;
using BookStore.KeywordSearch.Application.Search.DTOs;
using BookStore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BookStore.KeywordSearch.Application.Search.Queries.KeywordSearch;

/// <summary>
/// Handles keyword search using PostgreSQL full-text search with ts_rank scoring.
/// </summary>
internal sealed class KeywordSearchQueryHandler(
    IKeywordSearchDbContext context) : IQueryHandler<KeywordSearchQuery, Result<IReadOnlyList<SearchResultDto>>>
{
    /// <summary>
    /// Executes a full-text search against the SearchableBooks table.
    /// </summary>
    public async ValueTask<Result<IReadOnlyList<SearchResultDto>>> Handle(
        KeywordSearchQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
            return Result.Success<IReadOnlyList<SearchResultDto>>([]);

        var searchQuery = query.Query;
        var maxResults = query.MaxResults;

        var results = await context.SearchableBooks
            .FromSqlInterpolated($"""
                SELECT "Id", "Title", "Author", "Description", "IndexedAt"
                FROM "SearchableBooks"
                WHERE "SearchVector" @@ plainto_tsquery('english', {searchQuery})
                ORDER BY ts_rank("SearchVector", plainto_tsquery('english', {searchQuery})) DESC
                LIMIT {maxResults}
                """)
            .ToListAsync(cancellationToken);

        var dtos = results.Select((r, index) => new SearchResultDto(
            r.Id,
            r.Title,
            r.Author,
            r.Description,
            1.0 - (index * 0.01))).ToList();

        return Result.Success<IReadOnlyList<SearchResultDto>>(dtos);
    }
}
