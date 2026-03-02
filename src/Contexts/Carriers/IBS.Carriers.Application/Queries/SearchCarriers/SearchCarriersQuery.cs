using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Application.DTOs;

namespace IBS.Carriers.Application.Queries.SearchCarriers;

/// <summary>
/// Query to search carriers by name.
/// </summary>
/// <param name="SearchTerm">The search term.</param>
public sealed record SearchCarriersQuery(string SearchTerm) : IQuery<IReadOnlyList<CarrierSummaryDto>>;
