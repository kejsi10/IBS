using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;

namespace IBS.Carriers.Application.Queries.GetAllCarriers;

/// <summary>
/// Query to get all carriers.
/// </summary>
public sealed record GetAllCarriersQuery : IQuery<IReadOnlyList<CarrierSummaryDto>>;
