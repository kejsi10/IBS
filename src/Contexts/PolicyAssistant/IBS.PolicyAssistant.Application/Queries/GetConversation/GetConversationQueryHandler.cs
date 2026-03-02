using System.Text.Json;
using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Domain.Queries;

namespace IBS.PolicyAssistant.Application.Queries.GetConversation;

/// <summary>
/// Handler for the <see cref="GetConversationQuery"/>.
/// </summary>
public sealed class GetConversationQueryHandler(
    IConversationQueries queries) : IQueryHandler<GetConversationQuery, ConversationDetailsDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<Result<ConversationDetailsDto>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await queries.GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null)
            return Error.NotFound("Conversation", request.ConversationId);

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto(
                m.Id,
                m.Role,
                m.Content,
                m.MessageType,
                m.Metadata,
                m.CreatedAt))
            .ToList();

        PolicyExtractionResult? extractedData = null;
        if (!string.IsNullOrWhiteSpace(conversation.ExtractedData))
        {
            try
            {
                extractedData = JsonSerializer.Deserialize<PolicyExtractionResult>(
                    conversation.ExtractedData, JsonOptions);
            }
            catch (JsonException)
            {
                // Stored JSON is malformed; treat as no extraction
            }
        }

        return new ConversationDetailsDto(
            conversation.Id,
            conversation.Title,
            conversation.Mode,
            conversation.Status,
            conversation.LineOfBusiness,
            conversation.PolicyId,
            extractedData,
            messages,
            conversation.CreatedAt,
            conversation.UpdatedAt);
    }
}
