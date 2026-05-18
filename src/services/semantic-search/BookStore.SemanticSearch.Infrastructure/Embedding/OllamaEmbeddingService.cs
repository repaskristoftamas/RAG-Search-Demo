using BookStore.SemanticSearch.Application.Abstractions;
using OllamaSharp;

namespace BookStore.SemanticSearch.Infrastructure.Embedding;

/// <summary>
/// Generates embeddings using Ollama's embedding API.
/// </summary>
internal sealed class OllamaEmbeddingService(
    OllamaApiClient client,
    string modelName) : IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text using the configured Ollama model.
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var response = await client.EmbedAsync(new OllamaSharp.Models.EmbedRequest
        {
            Model = modelName,
            Input = [text]
        }, cancellationToken);

        return response.Embeddings[0];
    }
}
