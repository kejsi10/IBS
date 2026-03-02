namespace IBS.PolicyAssistant.Domain.Enums;

/// <summary>
/// Defines the mode of a policy assistant conversation.
/// </summary>
public enum ConversationMode
{
    /// <summary>
    /// The AI asks structured questions step by step to gather policy information.
    /// </summary>
    Guided,

    /// <summary>
    /// The user describes the policy requirements freely and the AI extracts the data.
    /// </summary>
    Freeform
}
