using BookStore.KeywordSearch.Application.Models;
using BookStore.KeywordSearch.Application.Search.Queries.KeywordSearch;
using BookStore.KeywordSearch.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookStore.KeywordSearch.IntegrationTests.Search;

[Collection(nameof(PostgreSqlCollection))]
public sealed class KeywordSearchQueryHandlerTests(PostgreSqlFixture fixture)
    : KeywordSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_MatchingQuery_ReturnsRankedResults()
    {
        await SeedBooks(
            ("Clean Architecture", "Robert Martin", "A guide to software structure and design patterns"),
            ("The Pragmatic Programmer", "David Thomas", "Practical tips for software developers"),
            ("Domain-Driven Design", "Eric Evans", "Tackling complexity in software with domain modeling"));

        await using var context = CreateContext();
        var handler = new KeywordSearchQueryHandler(context);
        var result = await handler.Handle(new KeywordSearchQuery("software"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.Value[0].RelevanceScore.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_TitleWeightedHigherThanDescription_TitleMatchRanksFirst()
    {
        await SeedBooks(
            ("Software Engineering", "Author A", "General overview of methods"),
            ("Cooking Recipes", "Author B", "A book about software and cooking techniques"));

        await using var context = CreateContext();
        var handler = new KeywordSearchQueryHandler(context);
        var result = await handler.Handle(new KeywordSearchQuery("software"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value[0].Title.ShouldBe("Software Engineering");
    }

    [Fact]
    public async Task Handle_CaseInsensitiveSearch_FindsResults()
    {
        await SeedBooks(("Clean Architecture", "Robert Martin", "A guide to software structure"));

        await using var context = CreateContext();
        var handler = new KeywordSearchQueryHandler(context);
        var result = await handler.Handle(new KeywordSearchQuery("CLEAN ARCHITECTURE"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Title.ShouldBe("Clean Architecture");
    }

    [Fact]
    public async Task Handle_NoMatchingBooks_ReturnsEmptyList()
    {
        await SeedBooks(("Clean Architecture", "Robert Martin", "A guide to software structure"));

        await using var context = CreateContext();
        var handler = new KeywordSearchQueryHandler(context);
        var result = await handler.Handle(new KeywordSearchQuery("quantum physics"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_EmptyOrWhitespaceQuery_ReturnsEmptyList(string? query)
    {
        await using var context = CreateContext();
        var handler = new KeywordSearchQueryHandler(context);
        var result = await handler.Handle(new KeywordSearchQuery(query!), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_MaxResultsLimit_RespectsLimit()
    {
        await SeedBooks(
            ("Software Design", "Author A", "Design patterns in software"),
            ("Software Testing", "Author B", "Testing strategies for software"),
            ("Software Architecture", "Author C", "Architectural patterns in software"),
            ("Software Metrics", "Author D", "Measuring software quality"));

        await using var context = CreateContext();
        var handler = new KeywordSearchQueryHandler(context);
        var result = await handler.Handle(new KeywordSearchQuery("software", MaxResults: 2), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    /// <summary>
    /// Seeds books into the test database.
    /// </summary>
    private async Task SeedBooks(params (string Title, string Author, string Description)[] books)
    {
        await using var context = CreateContext();
        foreach (var (title, author, description) in books)
        {
            context.SearchableBooks.Add(new SearchableBook
            {
                Id = Guid.NewGuid(),
                Title = title,
                Author = author,
                Description = description,
                IndexedAt = DateTimeOffset.UtcNow
            });
        }
        await context.SaveChangesAsync();
    }
}
