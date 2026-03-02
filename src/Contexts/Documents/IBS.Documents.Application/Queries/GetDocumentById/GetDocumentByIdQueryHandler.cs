using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentById;

/// <summary>
/// Handler for the GetDocumentByIdQuery.
/// </summary>
public sealed class GetDocumentByIdQueryHandler(
    IDocumentQueries documentQueries) : IQueryHandler<GetDocumentByIdQuery, DocumentReadModel>
{
    /// <inheritdoc />
    public async Task<Result<DocumentReadModel>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await documentQueries.GetByIdAsync(request.TenantId, request.DocumentId, cancellationToken);
        if (document is null)
            return Error.NotFound("Document not found.");

        return document;
    }
}
