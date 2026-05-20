using System.Net;
using System.Text;
using System.Text.Json;
using BookStore.Gateway.Comparison;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace BookStore.Gateway.UnitTests.Comparison;

public sealed class SearchServiceClientTests
{
    private static readonly Guid TestBookId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task SearchKeywordAsync_ValidResponse_ReturnsMappedResults()
    {
        var json = JsonSerializer.Serialize(new[]
        {
            new { BookId = TestBookId, Title = "Clean Code", Author = "Uncle Bob", Description = "Craftsmanship.", RelevanceScore = 0.95 }
        });
        var (client, _) = CreateClient("KeywordSearch", json);

        var result = await client.SearchKeywordAsync("clean code", 5, CancellationToken.None);

        result.Results.Count.ShouldBe(1);
        var item = result.Results[0];
        item.BookId.ShouldBe(TestBookId);
        item.Title.ShouldBe("Clean Code");
        item.Author.ShouldBe("Uncle Bob");
        item.Description.ShouldBe("Craftsmanship.");
        item.Score.ShouldBe(0.95);
        result.LlmSummary.ShouldBeNull();
    }

    [Fact]
    public async Task SearchKeywordAsync_EmptyResponse_ReturnsEmptyList()
    {
        var (client, _) = CreateClient("KeywordSearch", "[]");

        var result = await client.SearchKeywordAsync("nothing", 5, CancellationToken.None);

        result.Results.ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchKeywordAsync_EscapesQueryInUrl()
    {
        var (client, handler) = CreateClient("KeywordSearch", "[]");

        await client.SearchKeywordAsync("clean code & more", 5, CancellationToken.None);

        var uri = handler.LastRequest!.RequestUri!.AbsoluteUri;
        uri.ShouldContain("q=clean%20code%20%26%20more");
        uri.ShouldContain("maxResults=5");
    }

    [Fact]
    public async Task SearchSemanticAsync_ValidResponse_ReturnsMappedResultsWithSummary()
    {
        var json = JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = TestBookId, Title = "Clean Code", Author = "Uncle Bob", Description = "Craftsmanship.", SimilarityScore = 0.92 }
            },
            LlmSummary = "This book covers software craftsmanship."
        });
        var (client, _) = CreateClient("SemanticSearch", json);

        var result = await client.SearchSemanticAsync("clean code", 5, CancellationToken.None);

        result.Results.Count.ShouldBe(1);
        var item = result.Results[0];
        item.BookId.ShouldBe(TestBookId);
        item.Title.ShouldBe("Clean Code");
        item.Score.ShouldBe(0.92);
        result.LlmSummary.ShouldBe("This book covers software craftsmanship.");
    }

    [Fact]
    public async Task SearchSemanticAsync_NullLlmSummary_ReturnsNullSummary()
    {
        var json = JsonSerializer.Serialize(new
        {
            Results = new[]
            {
                new { BookId = TestBookId, Title = "Clean Code", Author = "Uncle Bob", Description = "Craftsmanship.", SimilarityScore = 0.92 }
            },
            LlmSummary = (string?)null
        });
        var (client, _) = CreateClient("SemanticSearch", json);

        var result = await client.SearchSemanticAsync("clean code", 5, CancellationToken.None);

        result.Results.ShouldNotBeEmpty();
        result.LlmSummary.ShouldBeNull();
    }

    [Fact]
    public async Task SearchSemanticAsync_EmptyResults_ReturnsEmptyList()
    {
        var json = JsonSerializer.Serialize(new { Results = Array.Empty<object>(), LlmSummary = (string?)null });
        var (client, _) = CreateClient("SemanticSearch", json);

        var result = await client.SearchSemanticAsync("nothing", 5, CancellationToken.None);

        result.Results.ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchSemanticAsync_IncludesLlmSummaryParam()
    {
        var json = JsonSerializer.Serialize(new { Results = Array.Empty<object>(), LlmSummary = (string?)null });
        var (client, handler) = CreateClient("SemanticSearch", json);

        await client.SearchSemanticAsync("test", 3, CancellationToken.None);

        var uri = handler.LastRequest!.RequestUri!.ToString();
        uri.ShouldContain("includeLlmSummary=true");
        uri.ShouldContain("maxResults=3");
    }

    /// <summary>
    /// Creates a <see cref="SearchServiceClient"/> backed by a mock HTTP handler for the given named client.
    /// </summary>
    private static (SearchServiceClient Client, MockHttpMessageHandler Handler) CreateClient(
        string clientName, string jsonResponse)
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test-host") };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(clientName)).Returns(httpClient);

        return (new SearchServiceClient(factory.Object, NullLogger<SearchServiceClient>.Instance), handler);
    }

    /// <summary>
    /// Captures the outgoing request and returns a canned response.
    /// </summary>
    private sealed class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(response);
        }
    }
}
