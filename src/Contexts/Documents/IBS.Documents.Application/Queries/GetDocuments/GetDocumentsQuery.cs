using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Aggregates.Document;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocuments;

/// <summary>
/// Query to get a paginated list of documents.
/// </summary>
public sealed record GetDocumentsQuery(
    Guid TenantId,
    string? SearchTerm = null,
    DocumentCategory? Category = null,
    DocumentEntityType? EntityType = null,
    Guid? EntityId = null,
    bool IncludeArchived = false,
    int Page = 1,
    int PageSize = 20
) : IQuery<DocumentSearchResult>;
