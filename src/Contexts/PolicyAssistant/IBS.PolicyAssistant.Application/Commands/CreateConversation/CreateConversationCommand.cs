using IBS.BuildingBlocks.Application.Commands;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Application.Commands.CreateConversation;

/// <summary>
/// Command to start a new policy assistant conversation.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="Title">The conversation title.</param>
/// <param name="Mode">The conversation mode (Guided or Freeform).</param>
/// <param name="LineOfBusiness">Optional initial line of business.</param>
public sealed record CreateConversationCommand(
    Guid TenantId,
    Guid UserId,
    string Title,
    ConversationMode Mode,
    string? LineOfBusiness = null) : ICommand<Guid>;
