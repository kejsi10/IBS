using System.Text;
using System.Text.Json;
using IBS.PolicyAssistant.Application.Services;
using Microsoft.Extensions.Options;

namespace IBS.PolicyAssistant.Infrastructure.Ai;

/// <summary>
/// Ollama implementation of <see cref="IChatCompletionService"/> using the /api/chat endpoint.
/// Used for local development with the llama3.1:8b model.
/// </summary>
public sealed class OllamaChatService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options) : IChatCompletionService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        var model = options.Value.ChatModel;

        var requestMessages = messages
            .Select(m => new { role = m.Role, content = m.Content })
            .ToArray();

        var body = new { model, messages = requestMessages, stream = false };

        var json = JsonSerializer.Serialize(body, SerializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/api/chat", content, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseBody);

        // Ollama chat response: { "message": { "role": "assistant", "content": "..." } }
        if (doc.RootElement.TryGetProperty("message", out var messageProp)
            && messageProp.TryGetProperty("content", out var contentProp))
        {
            return contentProp.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}
