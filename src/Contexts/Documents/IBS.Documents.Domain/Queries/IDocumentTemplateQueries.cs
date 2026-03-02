using IBS.Documents.Domain.Aggregates.DocumentTemplate;

namespace IBS.Documents.Domain.Queries;

/// <summary>
/// Read model for a document template.
/// </summary>
public sealed record DocumentTemplateReadModel(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    TemplateType TemplateType,
    string Content,
    bool IsActive,
    int Version,
    string CreatedBy);

/// <summary>
/// Read model for a document template list item.
/// </summary>
public sealed record DocumentTemplateListItemReadModel(
    Guid Id,
    string Name,
    string Description,
    TemplateType TemplateType,
    bool IsActive,
    int Version,
    string CreatedBy);

/// <summary>
/// Filter for searching document templates.
/// </summary>
public sealed record DocumentTemplateSearchFilter(
    Guid TenantId,
    string? SearchTerm = null,
    TemplateType? TemplateType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20);

/// <summary>
/// Result of a document template search.
/// </summary>
public sealed record DocumentTemplateSearchResult(
    IReadOnlyList<DocumentTemplateListItemReadModel> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Query interface for document templates.
/// </summary>
public interface IDocumentTemplateQueries
{
    /// <summary>
    /// Gets a template by its identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template read model if found, otherwise null.</returns>
    Task<DocumentTemplateReadModel?> GetByIdAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches templates with filters.
    /// </summary>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated search result.</returns>
    Task<DocumentTemplateSearchResult> SearchAsync(DocumentTemplateSearchFilter filter, CancellationToken cancellationToken = default);
}
