namespace IBS.PolicyAssistant.Domain.Enums;

/// <summary>
/// Defines the type of a message in a policy assistant conversation.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// A standard chat message between user and assistant.
    /// </summary>
    Chat,

    /// <summary>
    /// A message containing the AI-extracted policy data (JSON).
    /// </summary>
    PolicyExtraction,

    /// <summary>
    /// A message containing the AI validation result for the extracted policy.
    /// </summary>
    Validation
}
