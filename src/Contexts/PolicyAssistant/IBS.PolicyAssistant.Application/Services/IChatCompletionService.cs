namespace IBS.PolicyAssistant.Application.Services;

/// <summary>
/// Provider-agnostic interface for chat completion AI services.
/// Implementations exist for local Ollama (dev) and Azure OpenAI (prod).
/// </summary>
public interface IChatCompletionService
{
    /// <summary>
    /// Sends a conversation history to the AI and returns the assistant's response.
    /// </summary>
    /// <param name="messages">The ordered list of messages in the conversation (system, user, assistant turns).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The assistant's response text.</returns>
    Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct);
}

/// <summary>
/// Represents a single message in a chat conversation, provider-agnostic.
/// </summary>
/// <param name="Role">The message role: "system", "user", or "assistant".</param>
/// <param name="Content">The text content of the message.</param>
public sealed record ChatMessage(string Role, string Content);
