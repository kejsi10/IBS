using System.Text.Json;
using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Application.Services;
using IBS.PolicyAssistant.Domain.Enums;
using IBS.PolicyAssistant.Domain.Repositories;

namespace IBS.PolicyAssistant.Application.Commands.SendMessage;

/// <summary>
/// Handler for the <see cref="SendMessageCommand"/>.
/// Runs the full RAG pipeline: user message → doc search → build prompt → AI chat → extract → validate → persist.
/// </summary>
public sealed class SendMessageCommandHandler(
    IConversationRepository repository,
    IChatCompletionService chatService,
    IReferenceDocumentSearchService searchService,
    IPolicyExtractionService extractionService,
    IPolicyValidationService validationService,
    IUnitOfWork unitOfWork) : ICommandHandler<SendMessageCommand, SendMessageResponseDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<Result<SendMessageResponseDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Load the conversation
        var conversation = await repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            return Error.NotFound("Conversation", request.ConversationId);

        if (conversation.TenantId != request.TenantId || conversation.UserId != request.UserId)
            return Error.Forbidden();

        // Persist the user message immediately with concurrency retry.
        // This refreshes the RowVersion before the long-running AI calls.
        conversation.AddMessage("user", request.Content);
        await unitOfWork.SaveChangesWithConcurrencyRetryAsync(cancellationToken: cancellationToken);

        // Build the conversation history for the AI
        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Where(m => m.MessageType == MessageType.Chat)
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToList();

        // Search for relevant reference documents (RAG)
        var searchQuery = $"{request.Content} {conversation.LineOfBusiness}".Trim();
        var docs = await searchService.SearchAsync(
            searchQuery,
            lineOfBusiness: conversation.LineOfBusiness,
            ct: cancellationToken);

        // Build the chat messages with system prompt + retrieved context + conversation history
        var chatMessages = ChatPromptBuilder.Build(conversation.Mode, docs, history);

        // Get the AI response
        var aiResponse = await chatService.ChatAsync(chatMessages, cancellationToken);

        // Persist the assistant response
        conversation.AddMessage("assistant", aiResponse);

        // Extract structured policy data from the full conversation
        var allMessages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Where(m => m.MessageType == MessageType.Chat)
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToList();

        var extraction = await extractionService.ExtractAsync(allMessages, cancellationToken);

        // Update extracted data on the conversation
        var extractionJson = JsonSerializer.Serialize(extraction, JsonOptions);
        conversation.UpdateExtractedData(extractionJson, extraction.LineOfBusiness);

        // Run validation if we have enough data
        PolicyValidationResult? validation = null;
        if (!string.IsNullOrWhiteSpace(extraction.LineOfBusiness))
        {
            validation = await validationService.ValidateAsync(extraction, cancellationToken);
        }

        await unitOfWork.SaveChangesWithConcurrencyRetryAsync(cancellationToken: cancellationToken);

        // Find the persisted assistant message to return its ID
        var assistantMessage = conversation.Messages
            .OrderByDescending(m => m.CreatedAt)
            .First(m => m.Role == "assistant");

        return new SendMessageResponseDto(
            assistantMessage.Id,
            aiResponse,
            extraction,
            validation);
    }
}
