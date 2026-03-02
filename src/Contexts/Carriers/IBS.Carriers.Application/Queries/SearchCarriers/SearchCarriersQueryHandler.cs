using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;
using IBS.Carriers.Domain.Queries;

namespace IBS.Carriers.Application.Queries.SearchCarriers;

/// <summary>
/// Handler for the SearchCarriersQuery.
/// </summary>
public sealed class SearchCarriersQueryHandler(
    ICarrierQueries carrierQueries) : IQueryHandler<SearchCarriersQuery, IReadOnlyList<CarrierSummaryDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CarrierSummaryDto>>> Handle(
        SearchCarriersQuery request,
        CancellationToken cancellationToken)
    {
        var carriers = await carrierQueries.SearchByNameAsync(request.SearchTerm, cancellationToken);

        return carriers.Select(c => c.ToSummaryDto()).ToList();
    }
}
