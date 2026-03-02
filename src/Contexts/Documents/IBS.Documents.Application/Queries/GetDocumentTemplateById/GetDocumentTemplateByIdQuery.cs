using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentTemplateById;

/// <summary>
/// Query to get a document template by its identifier.
/// </summary>
public sealed record GetDocumentTemplateByIdQuery(
    Guid TenantId,
    Guid TemplateId
) : IQuery<DocumentTemplateReadModel>;
