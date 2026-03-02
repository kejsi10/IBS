using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetQuotes;

/// <summary>
/// Query to search and list quotes.
/// </summary>
public sealed record GetQuotesQuery(
    Guid TenantId,
    string? SearchTerm = null,
    Guid? ClientId = null,
    QuoteStatus? Status = null,
    LineOfBusiness? LineOfBusiness = null,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortDirection = "desc"
) : IQuery<QuoteSearchResult>;
