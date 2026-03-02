using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuoteById;

/// <summary>
/// Handler for the GetQuoteByIdQuery.
/// </summary>
public sealed class GetQuoteByIdQueryHandler(
    IQuoteQueries quoteQueries) : IQueryHandler<GetQuoteByIdQuery, QuoteDetailReadModel?>
{
    /// <inheritdoc />
    public async Task<Result<QuoteDetailReadModel?>> Handle(GetQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        var quote = await quoteQueries.GetByIdAsync(request.TenantId, request.QuoteId, cancellationToken);
        return quote;
    }
}
