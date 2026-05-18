using BookStore.Catalog.Api.Endpoints;
using BookStore.Catalog.Application;
using BookStore.Catalog.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync(app.Lifetime.ApplicationStopping);

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status500InternalServerError,
            title = "An unexpected error occurred."
        });
    });
});

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

app.MapBookEndpoints();

app.Run();

/// <summary>Integration test anchor for WebApplicationFactory.</summary>
public partial class Program;
