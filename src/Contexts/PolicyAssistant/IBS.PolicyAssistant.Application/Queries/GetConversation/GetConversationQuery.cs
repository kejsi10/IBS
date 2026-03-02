using IBS.BuildingBlocks.Application.Queries;
using IBS.PolicyAssistant.Application.DTOs;

namespace IBS.PolicyAssistant.Application.Queries.GetConversation;

/// <summary>
/// Query to get a conversation with all its messages.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
public sealed record GetConversationQuery(Guid ConversationId) : IQuery<ConversationDetailsDto>;
