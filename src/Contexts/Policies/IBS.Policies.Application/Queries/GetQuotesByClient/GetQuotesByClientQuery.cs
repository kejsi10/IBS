using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuotesByClient;

/// <summary>
/// Query to get quotes for a specific client.
/// </summary>
public sealed record GetQuotesByClientQuery(
    Guid TenantId,
    Guid ClientId,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<QuoteSearchResult>;
