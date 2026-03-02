using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Domain.Aggregates.Conversation;

/// <summary>
/// Represents a single message in a policy assistant conversation.
/// Child entity of the <see cref="Conversation"/> aggregate.
/// </summary>
public sealed class ChatMessageEntity : Entity
{
    /// <summary>
    /// Gets the conversation this message belongs to.
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// Gets the role of the message sender (user, assistant, system).
    /// </summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the text content of the message.
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the type of the message (Chat, PolicyExtraction, Validation).
    /// </summary>
    public MessageType MessageType { get; private set; }

    /// <summary>
    /// Gets additional JSON metadata associated with the message (e.g., extraction result, validation result).
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private ChatMessageEntity() { }

    /// <summary>
    /// Creates a new chat message entity.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <param name="role">The sender role.</param>
    /// <param name="content">The message content.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="metadata">Optional JSON metadata.</param>
    /// <returns>A new <see cref="ChatMessageEntity"/>.</returns>
    public static ChatMessageEntity Create(
        Guid conversationId,
        string role,
        string content,
        MessageType messageType = MessageType.Chat,
        string? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new ChatMessageEntity
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            MessageType = messageType,
            Metadata = metadata
        };
    }
}
