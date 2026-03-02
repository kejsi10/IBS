using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuotesSummary;

/// <summary>
/// Query to get quote summary statistics.
/// </summary>
public sealed record GetQuotesSummaryQuery(
    Guid TenantId
) : IQuery<QuoteSummaryStats>;
