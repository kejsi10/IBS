using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;

namespace IBS.PolicyAssistant.Domain.Repositories;

/// <summary>
/// Repository interface for the <see cref="ReferenceDocument"/> aggregate root.
/// </summary>
public interface IReferenceDocumentRepository : IRepository<ReferenceDocument>
{
}
