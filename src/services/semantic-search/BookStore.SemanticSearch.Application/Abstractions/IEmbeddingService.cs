namespace BookStore.SemanticSearch.Application.Abstractions;

/// <summary>
/// Generates vector embeddings from text input.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A float array representing the embedding vector.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
