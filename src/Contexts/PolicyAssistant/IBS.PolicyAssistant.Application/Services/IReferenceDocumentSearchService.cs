namespace IBS.PolicyAssistant.Application.Services;

/// <summary>
/// Provider-agnostic interface for searching reference documents.
/// Local: SQL Server full-text search. Azure: Azure AI Search.
/// </summary>
public interface IReferenceDocumentSearchService
{
    /// <summary>
    /// Searches reference documents relevant to the given query text.
    /// </summary>
    /// <param name="query">The search query text.</param>
    /// <param name="lineOfBusiness">Optional line of business filter.</param>
    /// <param name="state">Optional state filter.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of relevant document excerpts.</returns>
    Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(
        string query,
        string? lineOfBusiness = null,
        string? state = null,
        int maxResults = 5,
        CancellationToken ct = default);
}

/// <summary>
/// Represents a single search result from a reference document search.
/// </summary>
/// <param name="DocumentId">The reference document identifier.</param>
/// <param name="Title">The document title.</param>
/// <param name="Category">The document category.</param>
/// <param name="Content">The relevant text excerpt.</param>
/// <param name="Source">The source description.</param>
public sealed record DocumentSearchResult(
    Guid DocumentId,
    string Title,
    string Category,
    string Content,
    string? Source);
