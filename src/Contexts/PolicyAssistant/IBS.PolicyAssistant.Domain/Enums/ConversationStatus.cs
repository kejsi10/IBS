namespace IBS.PolicyAssistant.Domain.Enums;

/// <summary>
/// Defines the status of a policy assistant conversation.
/// </summary>
public enum ConversationStatus
{
    /// <summary>
    /// The conversation is in progress.
    /// </summary>
    Active,

    /// <summary>
    /// A policy has been created from this conversation.
    /// </summary>
    PolicyCreated,

    /// <summary>
    /// The conversation was abandoned without creating a policy.
    /// </summary>
    Abandoned
}
