using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuotes;

/// <summary>
/// Handler for the GetQuotesQuery.
/// </summary>
public sealed class GetQuotesQueryHandler(
    IQuoteQueries quoteQueries) : IQueryHandler<GetQuotesQuery, QuoteSearchResult>
{
    /// <inheritdoc />
    public async Task<Result<QuoteSearchResult>> Handle(GetQuotesQuery request, CancellationToken cancellationToken)
    {
        var filter = new QuoteSearchFilter
        {
            SearchTerm = request.SearchTerm,
            ClientId = request.ClientId,
            Status = request.Status,
            LineOfBusiness = request.LineOfBusiness,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var result = await quoteQueries.SearchAsync(request.TenantId, filter, cancellationToken);
        return result;
    }
}
