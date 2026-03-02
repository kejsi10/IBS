using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentById;

/// <summary>
/// Query to get a document by its identifier.
/// </summary>
public sealed record GetDocumentByIdQuery(
    Guid TenantId,
    Guid DocumentId
) : IQuery<DocumentReadModel>;
