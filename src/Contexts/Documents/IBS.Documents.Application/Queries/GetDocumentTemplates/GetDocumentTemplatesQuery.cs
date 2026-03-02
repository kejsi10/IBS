using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentTemplates;

/// <summary>
/// Query to get a paginated list of document templates.
/// </summary>
public sealed record GetDocumentTemplatesQuery(
    Guid TenantId,
    string? SearchTerm = null,
    TemplateType? TemplateType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<DocumentTemplateSearchResult>;
