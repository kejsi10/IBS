using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Aggregates.Carrier;

namespace IBS.Carriers.Domain.Repositories;

/// <summary>
/// Repository interface for the Carrier aggregate root.
/// </summary>
public interface ICarrierRepository : IRepository<Carrier>
{
}
