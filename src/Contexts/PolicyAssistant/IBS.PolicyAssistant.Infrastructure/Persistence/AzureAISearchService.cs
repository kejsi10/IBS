using IBS.PolicyAssistant.Application.Services;

namespace IBS.PolicyAssistant.Infrastructure.Persistence;

/// <summary>
/// Azure provider implementation of <see cref="IReferenceDocumentSearchService"/>.
/// Delegates to <see cref="SqlFullTextSearchService"/> to avoid the cost of Azure AI Search (~$100+/mo).
/// Can be swapped for a real Azure.Search.Documents implementation when budget allows.
/// </summary>
public sealed class AzureAISearchService(SqlFullTextSearchService inner) : IReferenceDocumentSearchService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(
        string query,
        string? lineOfBusiness = null,
        string? state = null,
        int maxResults = 5,
        CancellationToken ct = default)
        => inner.SearchAsync(query, lineOfBusiness, state, maxResults, ct);
}
