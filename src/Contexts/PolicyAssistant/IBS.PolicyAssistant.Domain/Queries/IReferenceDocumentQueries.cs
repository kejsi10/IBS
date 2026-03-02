using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using IBS.PolicyAssistant.Domain.Enums;

namespace IBS.PolicyAssistant.Domain.Queries;

/// <summary>
/// Query interface for reading <see cref="ReferenceDocument"/> data.
/// </summary>
public interface IReferenceDocumentQueries
{
    /// <summary>
    /// Gets all reference documents, optionally filtered by category.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of reference documents.</returns>
    Task<IReadOnlyList<ReferenceDocument>> GetAllAsync(DocumentCategory? category = null, CancellationToken cancellationToken = default);
}
