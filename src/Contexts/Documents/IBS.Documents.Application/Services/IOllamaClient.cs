namespace IBS.Documents.Application.Services;

/// <summary>
/// Client for interacting with an Ollama language model server.
/// Supports both text-only and vision (image-aware) generation.
/// </summary>
public interface IOllamaClient
{
    /// <summary>
    /// Generates a text response from the specified model using a text-only prompt.
    /// </summary>
    /// <param name="model">The model identifier (e.g., "qwen2.5:3b").</param>
    /// <param name="prompt">The prompt to send to the model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The model's text response.</returns>
    Task<string> GenerateAsync(string model, string prompt, CancellationToken ct);

    /// <summary>
    /// Generates a text response from a vision-capable model using a prompt and base64-encoded images.
    /// </summary>
    /// <param name="model">The model identifier (e.g., "qwen2.5:3b").</param>
    /// <param name="prompt">The prompt describing what to do with the images.</param>
    /// <param name="base64Images">Base64-encoded PNG images of document pages.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The model's text response.</returns>
    Task<string> GenerateWithImagesAsync(string model, string prompt, IReadOnlyList<string> base64Images, CancellationToken ct);
}
