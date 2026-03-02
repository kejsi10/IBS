using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Documents.Domain.Queries;

namespace IBS.Documents.Application.Queries.GetDocumentTemplateById;

/// <summary>
/// Handler for the GetDocumentTemplateByIdQuery.
/// </summary>
public sealed class GetDocumentTemplateByIdQueryHandler(
    IDocumentTemplateQueries templateQueries) : IQueryHandler<GetDocumentTemplateByIdQuery, DocumentTemplateReadModel>
{
    /// <inheritdoc />
    public async Task<Result<DocumentTemplateReadModel>> Handle(GetDocumentTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await templateQueries.GetByIdAsync(request.TenantId, request.TemplateId, cancellationToken);
        if (template is null)
            return Error.NotFound("Template not found.");

        return template;
    }
}
