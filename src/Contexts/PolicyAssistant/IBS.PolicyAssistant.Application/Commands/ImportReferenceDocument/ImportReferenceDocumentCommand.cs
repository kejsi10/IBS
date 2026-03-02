using IBS.BuildingBlocks.Application.Commands;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Application.Commands.ImportReferenceDocument;

/// <summary>
/// Command to import a new reference document for AI-assisted policy validation.
/// </summary>
/// <param name="TenantId">Optional tenant identifier (null for system-wide documents).</param>
/// <param name="Title">The document title.</param>
/// <param name="Category">The document category.</param>
/// <param name="Content">The full text content.</param>
/// <param name="LineOfBusiness">Optional applicable line of business.</param>
/// <param name="State">Optional applicable state code (e.g., "CA").</param>
/// <param name="Source">Optional source description.</param>
public sealed record ImportReferenceDocumentCommand(
    Guid? TenantId,
    string Title,
    DocumentCategory Category,
    string Content,
    string? LineOfBusiness = null,
    string? State = null,
    string? Source = null) : ICommand<Guid>;
