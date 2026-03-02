using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Application.DTOs;

/// <summary>
/// Summary DTO for a policy assistant conversation (list view).
/// </summary>
/// <param name="Id">The conversation identifier.</param>
/// <param name="Title">The conversation title.</param>
/// <param name="Mode">The conversation mode.</param>
/// <param name="Status">The current status.</param>
/// <param name="LineOfBusiness">The line of business being discussed.</param>
/// <param name="PolicyId">The policy identifier if created.</param>
/// <param name="MessageCount">The number of messages in the conversation.</param>
/// <param name="CreatedAt">When the conversation was started.</param>
/// <param name="UpdatedAt">When the conversation was last updated.</param>
public sealed record ConversationDto(
    Guid Id,
    string Title,
    ConversationMode Mode,
    ConversationStatus Status,
    string? LineOfBusiness,
    Guid? PolicyId,
    int MessageCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Detailed DTO for a policy assistant conversation with all messages.
/// </summary>
/// <param name="Id">The conversation identifier.</param>
/// <param name="Title">The conversation title.</param>
/// <param name="Mode">The conversation mode.</param>
/// <param name="Status">The current status.</param>
/// <param name="LineOfBusiness">The line of business being discussed.</param>
/// <param name="PolicyId">The policy identifier if created.</param>
/// <param name="ExtractedData">The extracted policy data, deserialized from stored JSON.</param>
/// <param name="Messages">All messages in the conversation.</param>
/// <param name="CreatedAt">When the conversation was started.</param>
/// <param name="UpdatedAt">When the conversation was last updated.</param>
public sealed record ConversationDetailsDto(
    Guid Id,
    string Title,
    ConversationMode Mode,
    ConversationStatus Status,
    string? LineOfBusiness,
    Guid? PolicyId,
    PolicyExtractionResult? ExtractedData,
    IReadOnlyList<ChatMessageDto> Messages,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// DTO for a single message in a conversation.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Role">The sender role (user, assistant, system).</param>
/// <param name="Content">The message content.</param>
/// <param name="MessageType">The message type.</param>
/// <param name="Metadata">Optional JSON metadata.</param>
/// <param name="CreatedAt">When the message was created.</param>
public sealed record ChatMessageDto(
    Guid Id,
    string Role,
    string Content,
    MessageType MessageType,
    string? Metadata,
    DateTimeOffset CreatedAt);
