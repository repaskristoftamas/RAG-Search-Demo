namespace BookStore.SemanticSearch.Application.Abstractions;

/// <summary>
/// Generates natural language text using a large language model.
/// </summary>
public interface ITextGenerationService
{
    /// <summary>
    /// Generates a response from the LLM given a prompt.
    /// </summary>
    /// <param name="prompt">The prompt to send to the model.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The generated text response.</returns>
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
