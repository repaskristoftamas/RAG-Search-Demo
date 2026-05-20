using BookStore.SemanticSearch.Application.Abstractions;
using BookStore.SemanticSearch.Application.Search.Queries.SemanticSearch;
using BookStore.SemanticSearch.IntegrationTests.Infrastructure;
using Shouldly;

namespace BookStore.SemanticSearch.IntegrationTests.Search;

[Collection(nameof(PostgreSqlCollection))]
public sealed class SemanticSearchQueryHandlerTests(PostgreSqlFixture fixture)
    : SemanticSearchIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Handle_MatchingQuery_ReturnsResults()
    {
        var queryEmbedding = CreateEmbedding((0, 1.0f));
        await InsertBookEmbeddingAsync(
            Guid.NewGuid(), "Clean Architecture", "Robert Martin",
            "A guide to software structure", queryEmbedding);

        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, CreateMockEmbeddingService(queryEmbedding).Object, Mock.Of<ITextGenerationService>())
            .Handle(new SemanticSearchQuery("software architecture"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.Count.ShouldBe(1);
        result.Value.Results[0].Title.ShouldBe("Clean Architecture");
        result.Value.Results[0].SimilarityScore.ShouldBeGreaterThan(0.9);
    }

    [Fact]
    public async Task Handle_NoMatchingBooks_ReturnsEmptyList()
    {
        var bookEmbedding = CreateEmbedding((1, 1.0f));
        await InsertBookEmbeddingAsync(
            Guid.NewGuid(), "Orthogonal Book", "Author", "Description", bookEmbedding);

        var queryEmbedding = CreateEmbedding((0, 1.0f));

        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, CreateMockEmbeddingService(queryEmbedding).Object, Mock.Of<ITextGenerationService>())
            .Handle(new SemanticSearchQuery("something unrelated"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_EmptyOrWhitespaceQuery_ReturnsEmptyResults(string? query)
    {
        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, Mock.Of<IEmbeddingService>(), Mock.Of<ITextGenerationService>())
            .Handle(new SemanticSearchQuery(query!), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.ShouldBeEmpty();
        result.Value.LlmSummary.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_MaxResultsLimit_RespectsLimit()
    {
        var embedding = CreateEmbedding((0, 1.0f));
        for (var i = 0; i < 5; i++)
            await InsertBookEmbeddingAsync(
                Guid.NewGuid(), $"Book {i}", $"Author {i}", $"Description {i}", embedding);

        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, CreateMockEmbeddingService(embedding).Object, Mock.Of<ITextGenerationService>())
            .Handle(new SemanticSearchQuery("books", MaxResults: 2), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_ResultsOrderedBySimilarity_MostSimilarFirst()
    {
        var queryEmbedding = CreateEmbedding((0, 1.0f));
        var closeEmbedding = CreateEmbedding((0, 1.0f));
        var furtherEmbedding = CreateEmbedding((0, 0.8f), (1, 0.6f));

        await InsertBookEmbeddingAsync(
            Guid.NewGuid(), "Further Match", "Author", "Description", furtherEmbedding);
        await InsertBookEmbeddingAsync(
            Guid.NewGuid(), "Exact Match", "Author", "Description", closeEmbedding);

        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, CreateMockEmbeddingService(queryEmbedding).Object, Mock.Of<ITextGenerationService>())
            .Handle(new SemanticSearchQuery("test query"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.Count.ShouldBe(2);
        result.Value.Results[0].Title.ShouldBe("Exact Match");
        result.Value.Results[0].SimilarityScore.ShouldBeGreaterThan(result.Value.Results[1].SimilarityScore);
    }

    [Fact]
    public async Task Handle_IncludeLlmSummary_ReturnsSummary()
    {
        var embedding = CreateEmbedding((0, 1.0f));
        await InsertBookEmbeddingAsync(
            Guid.NewGuid(), "Clean Code", "Uncle Bob", "Craftsmanship.", embedding);

        var mockTextGen = new Mock<ITextGenerationService>();
        mockTextGen.Setup(t => t.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("These books are relevant because they cover software design.");

        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, CreateMockEmbeddingService(embedding).Object, mockTextGen.Object)
            .Handle(new SemanticSearchQuery("clean code", IncludeLlmSummary: true), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.ShouldNotBeEmpty();
        result.Value.LlmSummary.ShouldBe("These books are relevant because they cover software design.");
    }

    [Fact]
    public async Task Handle_WithoutLlmSummary_ReturnsNullSummary()
    {
        var embedding = CreateEmbedding((0, 1.0f));
        await InsertBookEmbeddingAsync(
            Guid.NewGuid(), "Clean Code", "Uncle Bob", "Craftsmanship.", embedding);

        await using var context = CreateContext();
        var result = await new SemanticSearchQueryHandler(
                context, CreateMockEmbeddingService(embedding).Object, Mock.Of<ITextGenerationService>())
            .Handle(new SemanticSearchQuery("clean code"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Results.ShouldNotBeEmpty();
        result.Value.LlmSummary.ShouldBeNull();
    }

    private static Mock<IEmbeddingService> CreateMockEmbeddingService(float[] embedding)
    {
        var mock = new Mock<IEmbeddingService>();
        mock.Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);
        return mock;
    }
}
