using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;
using IBS.Carriers.Domain.Queries;

namespace IBS.Carriers.Application.Queries.GetAllCarriers;

/// <summary>
/// Handler for the GetAllCarriersQuery.
/// </summary>
public sealed class GetAllCarriersQueryHandler(
    ICarrierQueries carrierQueries) : IQueryHandler<GetAllCarriersQuery, IReadOnlyList<CarrierSummaryDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CarrierSummaryDto>>> Handle(
        GetAllCarriersQuery request,
        CancellationToken cancellationToken)
    {
        var carriers = await carrierQueries.GetAllAsync(cancellationToken);

        return carriers.Select(c => c.ToSummaryDto()).ToList();
    }
}
