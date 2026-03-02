using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentTemplates;

/// <summary>
/// Handler for the GetDocumentTemplatesQuery.
/// </summary>
public sealed class GetDocumentTemplatesQueryHandler(
    IDocumentTemplateQueries templateQueries) : IQueryHandler<GetDocumentTemplatesQuery, DocumentTemplateSearchResult>
{
    /// <inheritdoc />
    public async Task<Result<DocumentTemplateSearchResult>> Handle(GetDocumentTemplatesQuery request, CancellationToken cancellationToken)
    {
        var filter = new DocumentTemplateSearchFilter(
            request.TenantId,
            request.SearchTerm,
            request.TemplateType,
            request.IsActive,
            request.Page,
            request.PageSize);

        var result = await templateQueries.SearchAsync(filter, cancellationToken);
        return result;
    }
}
