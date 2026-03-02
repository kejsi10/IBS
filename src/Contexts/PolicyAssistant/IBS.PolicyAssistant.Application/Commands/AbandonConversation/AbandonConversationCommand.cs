using IBS.BuildingBlocks.Application.Commands;

namespace IBS.PolicyAssistant.Application.Commands.AbandonConversation;

/// <summary>
/// Command to abandon an active policy assistant conversation.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
public sealed record AbandonConversationCommand(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId) : ICommand;
