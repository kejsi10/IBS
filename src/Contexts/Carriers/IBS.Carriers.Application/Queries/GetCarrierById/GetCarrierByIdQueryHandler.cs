using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;
using IBS.Carriers.Domain.Queries;

namespace IBS.Carriers.Application.Queries.GetCarrierById;

/// <summary>
/// Handler for the GetCarrierByIdQuery.
/// </summary>
public sealed class GetCarrierByIdQueryHandler(
    ICarrierQueries carrierQueries) : IQueryHandler<GetCarrierByIdQuery, CarrierDto>
{
    /// <inheritdoc />
    public async Task<Result<CarrierDto>> Handle(GetCarrierByIdQuery request, CancellationToken cancellationToken)
    {
        var carrier = await carrierQueries.GetByIdAsync(request.CarrierId, cancellationToken);

        if (carrier is null)
        {
            return Error.NotFound("Carrier", request.CarrierId);
        }

        return carrier.ToDto();
    }
}
