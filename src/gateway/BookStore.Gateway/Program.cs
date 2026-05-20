using BookStore.Gateway.Comparison;
using BookStore.Gateway.HybridSearch;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var keywordBaseUrl = builder.Configuration["Services:KeywordSearchBaseUrl"]
    ?? "http://localhost:5020";
var semanticBaseUrl = builder.Configuration["Services:SemanticSearchBaseUrl"]
    ?? "http://localhost:5030";

builder.Services.AddHttpClient("KeywordSearch", client =>
    client.BaseAddress = new Uri(keywordBaseUrl));
builder.Services.AddHttpClient("SemanticSearch", client =>
    client.BaseAddress = new Uri(semanticBaseUrl));

builder.Services.AddSingleton<SearchServiceClient>();
builder.Services.AddSingleton<IRankFusionStrategy, ReciprocalRankFusionStrategy>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health").ExcludeFromDescription();
app.MapComparisonEndpoints();
app.MapHybridSearchEndpoints();
app.MapReverseProxy();

app.Run();

/// <summary>Integration test anchor for WebApplicationFactory.</summary>
public partial class Program;
