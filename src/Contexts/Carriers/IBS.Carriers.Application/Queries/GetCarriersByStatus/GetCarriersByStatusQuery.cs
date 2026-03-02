using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.Queries.GetCarriersByStatus;

/// <summary>
/// Query to get carriers by status.
/// </summary>
/// <param name="Status">The carrier status.</param>
public sealed record GetCarriersByStatusQuery(CarrierStatus Status) : IQuery<IReadOnlyList<CarrierSummaryDto>>;
