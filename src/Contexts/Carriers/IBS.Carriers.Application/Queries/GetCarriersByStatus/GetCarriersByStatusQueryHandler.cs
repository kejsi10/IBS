using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;
using IBS.Carriers.Domain.Queries;

namespace IBS.Carriers.Application.Queries.GetCarriersByStatus;

/// <summary>
/// Handler for the GetCarriersByStatusQuery.
/// </summary>
public sealed class GetCarriersByStatusQueryHandler(
    ICarrierQueries carrierQueries) : IQueryHandler<GetCarriersByStatusQuery, IReadOnlyList<CarrierSummaryDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CarrierSummaryDto>>> Handle(
        GetCarriersByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var carriers = await carrierQueries.GetByStatusAsync(request.Status, cancellationToken);

        return carriers.Select(c => c.ToSummaryDto()).ToList();
    }
}
