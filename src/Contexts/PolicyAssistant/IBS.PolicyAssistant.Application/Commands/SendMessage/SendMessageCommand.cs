using IBS.BuildingBlocks.Application.Commands;
using IBS.PolicyAssistant.Application.DTOs;

namespace IBS.PolicyAssistant.Application.Commands.SendMessage;

/// <summary>
/// Command to send a user message in a conversation and get the AI response.
/// Runs the full RAG pipeline: search docs → chat → extract → validate.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="Content">The user's message content.</param>
public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId,
    string Content) : ICommand<SendMessageResponseDto>;
