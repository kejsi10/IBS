using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuotesByClient;

/// <summary>
/// Handler for the GetQuotesByClientQuery.
/// </summary>
public sealed class GetQuotesByClientQueryHandler(
    IQuoteQueries quoteQueries) : IQueryHandler<GetQuotesByClientQuery, QuoteSearchResult>
{
    /// <inheritdoc />
    public async Task<Result<QuoteSearchResult>> Handle(GetQuotesByClientQuery request, CancellationToken cancellationToken)
    {
        var result = await quoteQueries.GetByClientIdAsync(
            request.TenantId,
            request.ClientId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return result;
    }
}
