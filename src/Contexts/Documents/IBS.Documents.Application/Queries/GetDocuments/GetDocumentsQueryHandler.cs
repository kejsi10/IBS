using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocuments;

/// <summary>
/// Handler for the GetDocumentsQuery.
/// </summary>
public sealed class GetDocumentsQueryHandler(
    IDocumentQueries documentQueries) : IQueryHandler<GetDocumentsQuery, DocumentSearchResult>
{
    /// <inheritdoc />
    public async Task<Result<DocumentSearchResult>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var filter = new DocumentSearchFilter(
            request.TenantId,
            request.SearchTerm,
            request.Category,
            request.EntityType,
            request.EntityId,
            request.IncludeArchived,
            request.Page,
            request.PageSize);

        var result = await documentQueries.SearchAsync(filter, cancellationToken);
        return result;
    }
}
