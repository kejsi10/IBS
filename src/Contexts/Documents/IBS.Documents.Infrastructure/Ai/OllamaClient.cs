using System.Text;
using System.Text.Json;
using IBS.Documents.Application.Services;

namespace IBS.Documents.Infrastructure.Ai;

/// <summary>
/// HTTP client implementation for the Ollama generate API.
/// Uses a typed HttpClient configured with the Ollama base URL and timeout.
/// </summary>
public sealed class OllamaClient(HttpClient httpClient) : IOllamaClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<string> GenerateAsync(string model, string prompt, CancellationToken ct)
    {
        var body = new { model, prompt, stream = false, think = false };
        return await PostAndExtractResponseAsync(body, ct);
    }

    /// <inheritdoc />
    public async Task<string> GenerateWithImagesAsync(
        string model,
        string prompt,
        IReadOnlyList<string> base64Images,
        CancellationToken ct)
    {
        var body = new { model, prompt, images = base64Images, stream = false, think = false };
        return await PostAndExtractResponseAsync(body, ct);
    }

    private async Task<string> PostAndExtractResponseAsync(object body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, SerializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/api/generate", content, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseBody);

        return doc.RootElement.TryGetProperty("response", out var prop)
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }
}
