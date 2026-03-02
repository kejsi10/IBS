using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Domain.Events;

/// <summary>
/// Raised when a new policy assistant conversation is created.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="Mode">The conversation mode.</param>
public sealed record ConversationCreatedEvent(
    Guid ConversationId,
    Guid TenantId,
    Guid UserId,
    ConversationMode Mode) : DomainEvent;

/// <summary>
/// Raised when a policy is created from a conversation.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="PolicyId">The created policy identifier.</param>
public sealed record ConversationPolicyCreatedEvent(
    Guid ConversationId,
    Guid TenantId,
    Guid PolicyId) : DomainEvent;
