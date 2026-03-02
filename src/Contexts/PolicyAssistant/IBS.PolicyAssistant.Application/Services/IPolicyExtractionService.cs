using IBS.PolicyAssistant.Application.DTOs;

namespace IBS.PolicyAssistant.Application.Services;

/// <summary>
/// Service for extracting structured policy data from conversation history.
/// Uses AI to parse the conversation and output a structured <see cref="PolicyExtractionResult"/>.
/// </summary>
public interface IPolicyExtractionService
{
    /// <summary>
    /// Extracts structured policy data from the given conversation messages.
    /// </summary>
    /// <param name="messages">The conversation messages to extract data from.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The extracted policy data, including any missing fields.</returns>
    Task<PolicyExtractionResult> ExtractAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct);
}
