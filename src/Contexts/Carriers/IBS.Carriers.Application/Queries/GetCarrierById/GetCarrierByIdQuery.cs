using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;

namespace IBS.Carriers.Application.Queries.GetCarrierById;

/// <summary>
/// Query to get a carrier by its identifier.
/// </summary>
/// <param name="CarrierId">The carrier identifier.</param>
public sealed record GetCarrierByIdQuery(Guid CarrierId) : IQuery<CarrierDto>;
