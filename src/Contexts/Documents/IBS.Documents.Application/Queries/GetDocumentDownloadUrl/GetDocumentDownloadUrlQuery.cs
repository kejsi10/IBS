using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentDownloadUrl;

/// <summary>
/// Query to get a temporary download URL for a document.
/// </summary>
public sealed record GetDocumentDownloadUrlQuery(
    Guid TenantId,
    Guid DocumentId
) : IQuery<string>;
