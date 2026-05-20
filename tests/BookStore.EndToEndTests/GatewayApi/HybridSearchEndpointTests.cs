using System.Net;
using System.Text.Json;
using BookStore.EndToEndTests.Infrastructure;
using Shouldly;

namespace BookStore.EndToEndTests.GatewayApi;

/// <summary>
/// Tests the hybrid search endpoint through the full HTTP pipeline with mock search backends.
/// </summary>
public sealed class HybridSearchEndpointTests(GatewayApiFactory factory) : IClassFixture<GatewayApiFactory>
{
    private static readonly Guid BookA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BookB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid BookC = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private readonly GatewayApiFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Search_BothServicesRespond_ReturnsFusedResults()
    {
        _factory.KeywordHandler.RespondWith(JsonSerializer.Serialize(new[]
        {
            new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", RelevanceScore = 0.95 },
            new { BookId = BookB, Title = "Book B", Author = "Author B", Description = "Desc B", RelevanceScore = 0.85 }
        }));

        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = BookB, Title = "Book B", Author = "Author B", Description = "Desc B", SimilarityScore = 0.92 },
                new { BookId = BookC, Title = "Book C", Author = "Author C", Description = "Desc C", SimilarityScore = 0.80 }
            },
            LlmSummary = "A summary."
        }));

        var response = await _client.GetAsync("/api/v1/search?q=test&maxResults=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        var results = root.GetProperty("results");

        results.GetArrayLength().ShouldBeGreaterThan(0);
        results[0].GetProperty("bookId").GetString().ShouldBe(BookB.ToString());
    }

    [Fact]
    public async Task Search_ReturnsCorrectResponseShape()
    {
        _factory.KeywordHandler.RespondWith(JsonSerializer.Serialize(new[]
        {
            new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", RelevanceScore = 0.9 }
        }));

        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", SimilarityScore = 0.8 }
            },
            LlmSummary = "Summary text."
        }));

        var response = await _client.GetAsync("/api/v1/search?q=test&maxResults=5");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        root.GetProperty("query").GetString().ShouldBe("test");
        root.GetProperty("totalCandidates").GetInt32().ShouldBeGreaterThan(0);
        root.GetProperty("llmSummary").GetString().ShouldBe("Summary text.");
        root.GetProperty("keywordElapsedMs").GetInt64().ShouldBeGreaterThanOrEqualTo(0);
        root.GetProperty("semanticElapsedMs").GetInt64().ShouldBeGreaterThanOrEqualTo(0);
        root.GetProperty("totalElapsedMs").GetInt64().ShouldBeGreaterThanOrEqualTo(0);

        var item = root.GetProperty("results")[0];
        item.GetProperty("bookId").ValueKind.ShouldBe(JsonValueKind.String);
        item.GetProperty("title").ValueKind.ShouldBe(JsonValueKind.String);
        item.GetProperty("author").ValueKind.ShouldBe(JsonValueKind.String);
        item.GetProperty("description").ValueKind.ShouldBe(JsonValueKind.String);
        item.GetProperty("fusedScore").ValueKind.ShouldBe(JsonValueKind.Number);
        item.GetProperty("keywordRank").ValueKind.ShouldBe(JsonValueKind.Number);
        item.GetProperty("semanticRank").ValueKind.ShouldBe(JsonValueKind.Number);
    }

    [Fact]
    public async Task Search_CustomWeights_AffectsOrder()
    {
        _factory.KeywordHandler.RespondWith(JsonSerializer.Serialize(new[]
        {
            new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", RelevanceScore = 0.95 }
        }));

        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = BookB, Title = "Book B", Author = "Author B", Description = "Desc B", SimilarityScore = 0.92 }
            },
            LlmSummary = (string?)null
        }));

        var keywordHeavy = await _client.GetAsync("/api/v1/search?q=test&maxResults=10&keywordWeight=0.9&semanticWeight=0.1");
        var semanticHeavy = await _client.GetAsync("/api/v1/search?q=test&maxResults=10&keywordWeight=0.1&semanticWeight=0.9");

        var kwDoc = JsonDocument.Parse(await keywordHeavy.Content.ReadAsStringAsync());
        var smDoc = JsonDocument.Parse(await semanticHeavy.Content.ReadAsStringAsync());

        kwDoc.RootElement.GetProperty("results")[0].GetProperty("bookId").GetString().ShouldBe(BookA.ToString());
        smDoc.RootElement.GetProperty("results")[0].GetProperty("bookId").GetString().ShouldBe(BookB.ToString());
    }

    [Fact]
    public async Task Search_MaxResultsLimitsOutput()
    {
        _factory.KeywordHandler.RespondWith(JsonSerializer.Serialize(new[]
        {
            new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", RelevanceScore = 0.9 },
            new { BookId = BookB, Title = "Book B", Author = "Author B", Description = "Desc B", RelevanceScore = 0.8 }
        }));

        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = BookC, Title = "Book C", Author = "Author C", Description = "Desc C", SimilarityScore = 0.85 }
            },
            LlmSummary = (string?)null
        }));

        var response = await _client.GetAsync("/api/v1/search?q=test&maxResults=2");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("results").GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public async Task Search_EmptyResults_ReturnsEmptyFusedList()
    {
        _factory.KeywordHandler.RespondWith("[]");
        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = Array.Empty<object>(),
            LlmSummary = (string?)null
        }));

        var response = await _client.GetAsync("/api/v1/search?q=nonexistent");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("results").GetArrayLength().ShouldBe(0);
        doc.RootElement.GetProperty("totalCandidates").GetInt32().ShouldBe(0);
    }

    [Fact]
    public async Task Search_KeywordServiceFails_Returns502()
    {
        _factory.KeywordHandler.RespondWithException();
        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = Array.Empty<object>(),
            LlmSummary = (string?)null
        }));

        var response = await _client.GetAsync("/api/v1/search?q=test");

        response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Search_SemanticServiceFails_Returns502()
    {
        _factory.KeywordHandler.RespondWith("[]");
        _factory.SemanticHandler.RespondWithException();

        var response = await _client.GetAsync("/api/v1/search?q=test");

        response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Search_PassesLlmSummaryThrough()
    {
        _factory.KeywordHandler.RespondWith(JsonSerializer.Serialize(new[]
        {
            new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", RelevanceScore = 0.9 }
        }));

        _factory.SemanticHandler.RespondWith(JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = BookA, Title = "Book A", Author = "Author A", Description = "Desc A", SimilarityScore = 0.8 }
            },
            LlmSummary = "These books cover software design."
        }));

        var response = await _client.GetAsync("/api/v1/search?q=design&maxResults=5");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("llmSummary").GetString().ShouldBe("These books cover software design.");
    }
}
