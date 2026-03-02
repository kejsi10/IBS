using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Application.Services;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentDownloadUrl;

/// <summary>
/// Handler for the GetDocumentDownloadUrlQuery.
/// </summary>
public sealed class GetDocumentDownloadUrlQueryHandler(
    IDocumentQueries documentQueries,
    IBlobStorageService blobStorageService) : IQueryHandler<GetDocumentDownloadUrlQuery, string>
{
    /// <inheritdoc />
    public async Task<Result<string>> Handle(GetDocumentDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var document = await documentQueries.GetByIdAsync(request.TenantId, request.DocumentId, cancellationToken);
        if (document is null)
            return Error.NotFound("Document not found.");

        var url = await blobStorageService.GetTemporaryDownloadUrlAsync(document.BlobKey, TimeSpan.FromHours(1), cancellationToken);
        return url;
    }
}
