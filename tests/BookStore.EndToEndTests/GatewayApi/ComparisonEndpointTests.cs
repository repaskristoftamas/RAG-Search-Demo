using System.Net;
using System.Text.Json;
using BookStore.EndToEndTests.Infrastructure;
using Shouldly;

namespace BookStore.EndToEndTests.GatewayApi;

/// <summary>
/// Tests the Gateway comparison endpoint through the full HTTP pipeline with mock search backends.
/// </summary>
public sealed class ComparisonEndpointTests(GatewayApiFactory factory) : IClassFixture<GatewayApiFactory>
{
    private static readonly Guid TestBookId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly GatewayApiFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Compare_BothServicesRespond_ReturnsComparisonResponse()
    {
        _factory.KeywordHandler.RespondWith(JsonSerializer.Serialize(new[]
        {
            new { BookId = TestBookId, Title = "Clean Code", Author = "Uncle Bob", Description = "Craftsmanship.", RelevanceScore = 0.95 }
        }));

        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = TestBookId, Title = "Clean Code", Author = "Uncle Bob", Description = "Craftsmanship.", SimilarityScore = 0.88 }
            },
            LlmSummary = "Relevant to software quality."
        }));

        var response = await _client.GetAsync("/api/v1/compare?q=clean+code&maxResults=5");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        root.GetProperty("query").GetString().ShouldBe("clean code");
        root.GetProperty("totalElapsedMs").GetInt64().ShouldBeGreaterThanOrEqualTo(0);

        var keyword = root.GetProperty("keywordResults");
        keyword.GetProperty("results").GetArrayLength().ShouldBe(1);
        keyword.GetProperty("results")[0].GetProperty("title").GetString().ShouldBe("Clean Code");
        keyword.GetProperty("results")[0].GetProperty("score").GetDouble().ShouldBe(0.95);

        var semantic = root.GetProperty("semanticResults");
        semantic.GetProperty("results").GetArrayLength().ShouldBe(1);
        semantic.GetProperty("results")[0].GetProperty("score").GetDouble().ShouldBe(0.88);
        semantic.GetProperty("llmSummary").GetString().ShouldBe("Relevant to software quality.");
    }

    [Fact]
    public async Task Compare_EmptyResults_ReturnsEmptyLists()
    {
        _factory.KeywordHandler.RespondWith("[]");
        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = Array.Empty<object>(),
            LlmSummary = (string?)null
        }));

        var response = await _client.GetAsync("/api/v1/compare?q=nonexistent");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("keywordResults").GetProperty("results").GetArrayLength().ShouldBe(0);
        doc.RootElement.GetProperty("semanticResults").GetProperty("results").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task Compare_KeywordServiceFails_Returns502()
    {
        _factory.KeywordHandler.RespondWithException();
        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = Array.Empty<object>(),
            LlmSummary = (string?)null
        }));

        var response = await _client.GetAsync("/api/v1/compare?q=test");

        response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Compare_SemanticServiceFails_Returns502()
    {
        _factory.KeywordHandler.RespondWith("[]");
        _factory.SemanticHandler.RespondWithException();

        var response = await _client.GetAsync("/api/v1/compare?q=test");

        response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    }
}
