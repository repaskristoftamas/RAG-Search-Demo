using System.Net;
using System.Text;
using BookStore.Gateway.Comparison;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.EndToEndTests.Infrastructure;

/// <summary>
/// Creates a test server for the Gateway with configurable mock backends for keyword and semantic search.
/// </summary>
public sealed class GatewayApiFactory : WebApplicationFactory<SearchServiceClient>
{
    /// <summary>
    /// Handler that intercepts keyword search HTTP requests.
    /// </summary>
    public MockHttpHandler KeywordHandler { get; } = new();

    /// <summary>
    /// Handler that intercepts semantic search HTTP requests.
    /// </summary>
    public MockHttpHandler SemanticHandler { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient("KeywordSearch", c => c.BaseAddress = new Uri("http://keyword-test"))
                .ConfigurePrimaryHttpMessageHandler(() => KeywordHandler);

            services.AddHttpClient("SemanticSearch", c => c.BaseAddress = new Uri("http://semantic-test"))
                .ConfigurePrimaryHttpMessageHandler(() => SemanticHandler);
        });
    }
}

/// <summary>
/// An <see cref="HttpMessageHandler"/> that returns a configurable canned response.
/// </summary>
public sealed class MockHttpHandler : HttpMessageHandler
{
    private HttpResponseMessage _response = new(HttpStatusCode.OK)
    {
        Content = new StringContent("[]", Encoding.UTF8, "application/json")
    };

    private Exception? _exception;

    /// <summary>
    /// Configures the handler to return the specified JSON body with 200 OK.
    /// </summary>
    public void RespondWith(string json)
    {
        _exception = null;
        _response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Configures the handler to throw an <see cref="HttpRequestException"/> on the next request.
    /// </summary>
    public void RespondWithException()
    {
        _exception = new HttpRequestException("Service unavailable");
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _exception is not null
            ? Task.FromException<HttpResponseMessage>(_exception)
            : Task.FromResult(_response);
    }
}
