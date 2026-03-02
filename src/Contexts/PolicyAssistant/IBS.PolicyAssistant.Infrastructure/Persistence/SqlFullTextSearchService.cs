using IBS.PolicyAssistant.Application.Services;
using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using Microsoft.EntityFrameworkCore;

namespace IBS.PolicyAssistant.Infrastructure.Persistence;

/// <summary>
/// SQL Server implementation of <see cref="IReferenceDocumentSearchService"/> using in-memory LIKE-based search.
/// For production, swap to <see cref="AzureAISearchService"/> via Azure AI Search full-text indexing.
/// </summary>
public sealed class SqlFullTextSearchService(DbContext context) : IReferenceDocumentSearchService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(
        string query,
        string? lineOfBusiness = null,
        string? state = null,
        int maxResults = 5,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        // Build search terms from query (words > 3 chars)
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 3)
            .Take(5)
            .ToArray();

        if (terms.Length == 0)
            return [];

        // Load documents with chunks from DB
        var documentsQuery = context.Set<ReferenceDocument>()
            .AsNoTracking()
            .Include(d => d.Chunks)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(lineOfBusiness))
            documentsQuery = documentsQuery.Where(d => d.LineOfBusiness == null || d.LineOfBusiness == lineOfBusiness);

        if (!string.IsNullOrWhiteSpace(state))
            documentsQuery = documentsQuery.Where(d => d.State == null || d.State == state);

        var allDocs = await documentsQuery.ToListAsync(ct);

        // Score each chunk by how many search terms it contains
        var results = new List<(DocumentSearchResult Result, int Score)>();

        foreach (var doc in allDocs)
        {
            foreach (var chunk in doc.Chunks)
            {
                var score = terms.Count(term =>
                    chunk.Content.Contains(term, StringComparison.OrdinalIgnoreCase));

                if (score > 0)
                {
                    results.Add((new DocumentSearchResult(
                        doc.Id,
                        doc.Title,
                        doc.Category.ToString(),
                        chunk.Content,
                        doc.Source), score));
                }
            }
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(maxResults)
            .Select(r => r.Result)
            .ToList();
    }
}
