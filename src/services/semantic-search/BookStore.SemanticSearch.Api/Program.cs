using BookStore.SemanticSearch.Api.Endpoints;
using BookStore.SemanticSearch.Application;
using BookStore.SemanticSearch.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync(app.Lifetime.ApplicationStopping);

app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
}).ExcludeFromDescription();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).ExcludeFromDescription();

app.MapSearchEndpoints();

app.Run();
