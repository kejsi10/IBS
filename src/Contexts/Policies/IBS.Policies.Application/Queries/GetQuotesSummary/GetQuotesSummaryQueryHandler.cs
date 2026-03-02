using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuotesSummary;

/// <summary>
/// Handler for the GetQuotesSummaryQuery.
/// </summary>
public sealed class GetQuotesSummaryQueryHandler(
    IQuoteQueries quoteQueries) : IQueryHandler<GetQuotesSummaryQuery, QuoteSummaryStats>
{
    /// <inheritdoc />
    public async Task<Result<QuoteSummaryStats>> Handle(GetQuotesSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await quoteQueries.GetSummaryAsync(request.TenantId, cancellationToken);
        return summary;
    }
}
