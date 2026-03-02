using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Queries;

namespace IBS.Policies.Application.Queries.GetQuoteById;

/// <summary>
/// Query to get a quote by its identifier.
/// </summary>
public sealed record GetQuoteByIdQuery(
    Guid TenantId,
    Guid QuoteId
) : IQuery<QuoteDetailReadModel?>;
