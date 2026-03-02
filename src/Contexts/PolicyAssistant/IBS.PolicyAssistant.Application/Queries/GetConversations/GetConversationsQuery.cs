using IBS.BuildingBlocks.Application.Queries;
using IBS.PolicyAssistant.Application.DTOs;

namespace IBS.PolicyAssistant.Application.Queries.GetConversations;

/// <summary>
/// Query to get all conversations for the current user.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
public sealed record GetConversationsQuery(
    Guid TenantId,
    Guid UserId) : IQuery<IReadOnlyList<ConversationDto>>;
