using IBS.BuildingBlocks.Application.Queries;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Application.Queries.SearchReferenceDocuments;

/// <summary>
/// Query to list reference documents (admin use).
/// </summary>
/// <param name="Category">Optional category filter.</param>
public sealed record SearchReferenceDocumentsQuery(
    DocumentCategory? Category = null) : IQuery<IReadOnlyList<ReferenceDocumentDto>>;
