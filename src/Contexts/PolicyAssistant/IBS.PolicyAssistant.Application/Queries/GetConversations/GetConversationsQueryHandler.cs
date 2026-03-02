using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Domain.Queries;

namespace IBS.PolicyAssistant.Application.Queries.GetConversations;

/// <summary>
/// Handler for the <see cref="GetConversationsQuery"/>.
/// </summary>
public sealed class GetConversationsQueryHandler(
    IConversationQueries queries) : IQueryHandler<GetConversationsQuery, IReadOnlyList<ConversationDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await queries.GetByUserAsync(request.TenantId, request.UserId, cancellationToken);

        var dtos = conversations
            .Select(c => new ConversationDto(
                c.Id,
                c.Title,
                c.Mode,
                c.Status,
                c.LineOfBusiness,
                c.PolicyId,
                c.Messages.Count,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

        return dtos;
    }
}
