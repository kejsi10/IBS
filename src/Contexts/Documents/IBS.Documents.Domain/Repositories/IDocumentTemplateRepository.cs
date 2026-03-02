using IBS.Documents.Domain.Aggregates.DocumentTemplate;

namespace IBS.Documents.Domain.Repositories;

/// <summary>
/// Repository interface for the DocumentTemplate aggregate.
/// </summary>
public interface IDocumentTemplateRepository
{
    /// <summary>
    /// Gets a template by its identifier.
    /// </summary>
    /// <param name="id">The template identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The template if found, otherwise null.</returns>
    Task<DocumentTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all templates for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All templates for the tenant.</returns>
    Task<IReadOnlyList<DocumentTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active templates by type.
    /// </summary>
    /// <param name="templateType">The template type to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Active templates of the specified type.</returns>
    Task<IReadOnlyList<DocumentTemplate>> GetActiveByTypeAsync(TemplateType templateType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new template.
    /// </summary>
    /// <param name="template">The template to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(DocumentTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    /// <param name="template">The template to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(DocumentTemplate template, CancellationToken cancellationToken = default);
}
