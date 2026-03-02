using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Application.Services;

/// <summary>
/// Builds the structured chat message list sent to the AI, combining system prompt,
/// retrieved reference document context (RAG), and conversation history.
/// </summary>
public static class ChatPromptBuilder
{
    private const string GuidedSystemPrompt = """
        You are an expert insurance broker assistant helping create insurance policies step by step.
        Your role is to ask one focused question at a time to collect the information needed for the policy.
        Ask for: client name, carrier, line of business, policy type, effective and expiration dates,
        billing type (Agency/Direct), payment plan, and coverage details (limits, deductibles, premiums).
        When you have all the information, summarize it clearly and ask the user to confirm.
        Be professional, concise, and helpful.
        """;

    private const string FreeformSystemPrompt = """
        You are an expert insurance broker assistant helping create insurance policies.
        The user will describe their policy requirements. Your role is to:
        1. Acknowledge what information was provided
        2. Ask for any missing critical details (client name, carrier, dates, coverages)
        3. Confirm your understanding of the complete policy before it is created
        Be professional, concise, and helpful.
        """;

    private const string ContextHeader = "\n\n--- RELEVANT INSURANCE REGULATIONS AND RULES ---\n";
    private const string ContextFooter = "\n--- END OF REFERENCE DOCUMENTS ---\n";

    /// <summary>
    /// Builds the full list of chat messages including system prompt, reference document context,
    /// and conversation history for sending to the AI.
    /// </summary>
    /// <param name="mode">The conversation mode (Guided or Freeform).</param>
    /// <param name="referenceDocuments">Retrieved reference document excerpts (RAG context).</param>
    /// <param name="conversationHistory">The ordered conversation history (user and assistant turns).</param>
    /// <returns>The complete ordered list of messages to send to the AI.</returns>
    public static IReadOnlyList<ChatMessage> Build(
        ConversationMode mode,
        IReadOnlyList<DocumentSearchResult> referenceDocuments,
        IReadOnlyList<ChatMessage> conversationHistory)
    {
        var messages = new List<ChatMessage>();

        // System prompt based on mode
        var systemPrompt = mode == ConversationMode.Guided ? GuidedSystemPrompt : FreeformSystemPrompt;

        // Append retrieved reference document context to system prompt
        if (referenceDocuments.Count > 0)
        {
            var contextBuilder = new System.Text.StringBuilder(systemPrompt);
            contextBuilder.Append(ContextHeader);

            foreach (var doc in referenceDocuments)
            {
                contextBuilder.AppendLine($"[{doc.Category}] {doc.Title}");
                if (!string.IsNullOrWhiteSpace(doc.Source))
                    contextBuilder.AppendLine($"Source: {doc.Source}");
                contextBuilder.AppendLine(doc.Content);
                contextBuilder.AppendLine();
            }

            contextBuilder.Append(ContextFooter);
            messages.Add(new ChatMessage("system", contextBuilder.ToString()));
        }
        else
        {
            messages.Add(new ChatMessage("system", systemPrompt));
        }

        // Add conversation history
        messages.AddRange(conversationHistory);

        return messages.AsReadOnly();
    }
}
