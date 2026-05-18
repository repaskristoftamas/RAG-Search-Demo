using System.Text;
using BookStore.SemanticSearch.Application.Abstractions;
using OllamaSharp;

namespace BookStore.SemanticSearch.Infrastructure.Embedding;

/// <summary>
/// Generates text using Ollama's chat/generate API.
/// </summary>
internal sealed class OllamaTextGenerationService(
    OllamaApiClient client,
    string modelName) : ITextGenerationService
{
    /// <summary>
    /// Generates a response from the LLM given a prompt.
    /// </summary>
    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        await foreach (var chunk in client.GenerateAsync(new OllamaSharp.Models.GenerateRequest
        {
            Model = modelName,
            Prompt = prompt
        }).WithCancellation(cancellationToken))
        {
            if (chunk?.Response is not null)
                sb.Append(chunk.Response);
        }

        return sb.ToString();
    }
}
