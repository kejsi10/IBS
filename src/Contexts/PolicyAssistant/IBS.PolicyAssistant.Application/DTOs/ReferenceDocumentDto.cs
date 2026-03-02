using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Application.DTOs;

/// <summary>
/// DTO for a reference document (admin view).
/// </summary>
/// <param name="Id">The document identifier.</param>
/// <param name="Title">The document title.</param>
/// <param name="Category">The document category.</param>
/// <param name="LineOfBusiness">The applicable line of business.</param>
/// <param name="State">The applicable state.</param>
/// <param name="Source">The document source.</param>
/// <param name="ChunkCount">The number of text chunks.</param>
/// <param name="CreatedAt">When the document was imported.</param>
public sealed record ReferenceDocumentDto(
    Guid Id,
    string Title,
    DocumentCategory Category,
    string? LineOfBusiness,
    string? State,
    string? Source,
    int ChunkCount,
    DateTimeOffset CreatedAt);

/// <summary>
/// DTO for the response to a send-message request.
/// </summary>
/// <param name="MessageId">The AI response message identifier.</param>
/// <param name="Content">The AI response text.</param>
/// <param name="Extraction">The updated policy extraction result.</param>
/// <param name="Validation">The validation result for the extracted data.</param>
public sealed record SendMessageResponseDto(
    Guid MessageId,
    string Content,
    PolicyExtractionResult? Extraction,
    PolicyValidationResult? Validation);
