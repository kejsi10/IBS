using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.PolicyAssistant.Application.DTOs;
using IBS.PolicyAssistant.Domain.Queries;

namespace IBS.PolicyAssistant.Application.Queries.SearchReferenceDocuments;

/// <summary>
/// Handler for the <see cref="SearchReferenceDocumentsQuery"/>.
/// </summary>
public sealed class SearchReferenceDocumentsQueryHandler(
    IReferenceDocumentQueries queries) : IQueryHandler<SearchReferenceDocumentsQuery, IReadOnlyList<ReferenceDocumentDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ReferenceDocumentDto>>> Handle(SearchReferenceDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await queries.GetAllAsync(request.Category, cancellationToken);

        var dtos = documents
            .Select(d => new ReferenceDocumentDto(
                d.Id,
                d.Title,
                d.Category,
                d.LineOfBusiness,
                d.State,
                d.Source,
                d.Chunks.Count,
                d.CreatedAt))
            .ToList();

        return dtos;
    }
}
